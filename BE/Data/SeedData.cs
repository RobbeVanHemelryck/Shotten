using BE.Models;
using BE.Services;
using Microsoft.EntityFrameworkCore;

namespace BE.Data;

public static class SeedData
{
    private static readonly string[] IcalUrls = { "https://www.lzvcup.be/icalendar.php?id=1319", "https://www.lzvcup.be/icalendar.php?id=2002" };
    private static readonly string[] ValidTeamNames = { "Wille ma ni kunne", "FC Degradé" };

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new ShottenContext(
                   serviceProvider.GetRequiredService<
                       DbContextOptions<ShottenContext>>()))
        {
            // Seed Teams
            if (!context.Teams.Any())
            {
                var teams = new Team[]
                {
                    new Team { Name = "Wille ma ni kunne" },
                    new Team { Name = "FC Degradé" }
                };
                context.Teams.AddRange(teams);
                await context.SaveChangesAsync();
            }

            var team1 = context.Teams.First(t => t.Name == "Wille ma ni kunne");
            var team2 = context.Teams.First(t => t.Name == "FC Degradé");

            // Seed Players
            if (!context.Players.Any())
            {
                var players = new Player[]
                {
                    new Player { Name = "Robbe", Teams = new List<Team> { team1, team2 } },
                    new Player { Name = "Lowie", Teams = new List<Team> { team1 } },
                    new Player { Name = "Ignace", Teams = new List<Team> { team1 } },
                    new Player { Name = "Thibaut", Teams = new List<Team> { team1 } },
                    new Player { Name = "Mathias", Teams = new List<Team> { team1 } },
                    new Player { Name = "Othman", Teams = new List<Team> { team2 } },
                    new Player { Name = "Tristan", Teams = new List<Team> { team2 } },
                    new Player { Name = "Lars", Teams = new List<Team> { team2 } },
                    new Player { Name = "Pieter", Teams = new List<Team> { team2 } },
                    new Player { Name = "Niels", Teams = new List<Team> { team2 } },
                    new Player { Name = "Senne", Teams = new List<Team> { team1, team2 } }
                };

                context.Players.AddRange(players);
                await context.SaveChangesAsync();
            }

            // Seed Matches from iCal
            if (!context.Matches.Any())
            {
                var icalService = serviceProvider.GetRequiredService<IcalService>();
                var allIcalEvents = new List<IcalService.IcalEvent>();

                foreach (var url in IcalUrls)
                {
                    var icalEvents = await icalService.GetIcalEvents(url);
                    allIcalEvents.AddRange(icalEvents);
                }

                foreach (var icalEvent in allIcalEvents)
                {
                    string teamName = "Unknown";
                    Team? matchTeam = null;

                    foreach (var validTeamName in ValidTeamNames)
                    {
                        if (icalEvent.Summary.Contains(validTeamName))
                        {
                            teamName = validTeamName;
                            matchTeam = context.Teams.FirstOrDefault(t => t.Name == validTeamName);
                            break;
                        }
                    }
                    if (teamName != "Unknown" && matchTeam != null)
                    {
                        var match = new Match
                        {
                            Date = icalEvent.StartDate,
                            Location = icalEvent.Location.Replace("\\", "").Replace(",", ",").Trim(),
                            Name = icalEvent.Summary, // Use raw event name
                            TeamName = teamName,
                            Team = matchTeam
                        };
                        if (!context.Matches.Any(m => m.Date == match.Date && m.Name == match.Name && m.Location == match.Location && m.TeamName == match.TeamName))
                        {
                            context.Matches.Add(match);
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}