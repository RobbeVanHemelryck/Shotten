using BE.Data;
using BE.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class MatchSyncService
{
    private readonly ShottenContext _context;
    private readonly IcalService _icalService;
    private readonly ILogger<MatchSyncService> _logger;

    private static readonly string[] IcalUrls = { "https://www.lzvcup.be/icalendar.php?id=1319", "https://www.lzvcup.be/icalendar.php?id=2002" };
    private static readonly string[] ValidTeamNames = { "Wille ma ni kunne", "FC Degradé" };

    public MatchSyncService(ShottenContext context, IcalService icalService, ILogger<MatchSyncService> logger)
    {
        _context = context;
        _icalService = icalService;
        _logger = logger;
    }

    public async Task SyncMatchesAsync()
    {
        _logger.LogInformation("Starting match synchronization...");

        // 1. Fetch all iCal events
        var allIcalEvents = await FetchAllIcalEventsAsync();

        // 2. Fetch necessary DB data
        var teams = await _context.Teams
            .Where(t => ValidTeamNames.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name);

        var existingMatches = await _context.Matches
            .Where(m => ValidTeamNames.Contains(m.TeamName))
            .ToListAsync();

        // 3. Process events into Match objects
        var incomingMatches = MapEventsToMatches(allIcalEvents, teams);

        // 4. Sync Changes
        await ApplyChangesAsync(existingMatches, incomingMatches);

        _logger.LogInformation("Match synchronization completed.");
    }

    private async Task<List<IcalService.IcalEvent>> FetchAllIcalEventsAsync()
    {
        var allEvents = new List<IcalService.IcalEvent>();
        foreach (var url in IcalUrls)
        {
            try
            {
                var events = await _icalService.GetIcalEvents(url);
                allEvents.AddRange(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching iCal events from {Url}", url);
            }
        }
        return allEvents;
    }

    private List<Match> MapEventsToMatches(List<IcalService.IcalEvent> events, Dictionary<string, Team> teams)
    {
        var matches = new List<Match>();

        foreach (var icalEvent in events)
        {
            var teamName = ValidTeamNames.FirstOrDefault(name => icalEvent.Summary.Contains(name));
            
            if (teamName != null && teams.TryGetValue(teamName, out var team))
            {
                matches.Add(new Match
                {
                    Date = icalEvent.StartDate,
                    Location = icalEvent.Location.Replace("\\", "").Replace(",", ",").Trim(),
                    Name = icalEvent.Summary,
                    TeamName = teamName,
                    Team = team,
                    TeamId = team.Id
                });
            }
        }
        return matches;
    }

    private async Task ApplyChangesAsync(List<Match> existingMatches, List<Match> incomingMatches)
    {
        var matchesToAdd = new List<Match>();
        var matchesToRemove = new List<Match>();

        // Identify matches to add
        foreach (var incoming in incomingMatches)
        {
            if (!existingMatches.Any(existing => IsSameMatch(existing, incoming)))
            {
                matchesToAdd.Add(incoming);
            }
        }

        // Identify matches to remove
        foreach (var existing in existingMatches)
        {
            if (!incomingMatches.Any(incoming => IsSameMatch(incoming, existing)))
            {
                matchesToRemove.Add(existing);
            }
        }

        if (matchesToAdd.Any())
        {
            _context.Matches.AddRange(matchesToAdd);
            _logger.LogInformation("Adding {Count} new matches.", matchesToAdd.Count);
        }

        if (matchesToRemove.Any())
        {
            _context.Matches.RemoveRange(matchesToRemove);
            _logger.LogInformation("Removing {Count} deleted matches.", matchesToRemove.Count);
        }

        if (matchesToAdd.Any() || matchesToRemove.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private bool IsSameMatch(Match a, Match b)
    {
        // Location is excluded from matching as per requirements
        return a.Date == b.Date && 
               a.Name == b.Name && 
               a.TeamName == b.TeamName;
    }
}
