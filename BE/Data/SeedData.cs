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
            // Seed Players
            if (!context.Players.Any())
            {
                var players = new Player[]
                {
                    new Player { Name = "Robbe" },
                    new Player { Name = "Lowie" },
                    new Player { Name = "Ignace" },
                    new Player { Name = "Thibaut" },
                    new Player { Name = "Mathias" },
                    new Player { Name = "Othman" },
                    new Player { Name = "Tristan" },
                    new Player { Name = "Lars" },
                    new Player { Name = "Pieter" },
                    new Player { Name = "Niels" },
                    new Player { Name = "Senne" }
                };

                foreach (Player p in players)
                {
                    context.Players.Add(p);
                }
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
                    foreach (var validTeamName in ValidTeamNames)
                    {
                        if (icalEvent.Summary.Contains(validTeamName))
                        {
                            teamName = validTeamName;
                            break;
                        }
                    }
                    if (teamName != "Unknown")
                    {
                        var match = new Match
                        {
                            Date = icalEvent.StartDate,
                            Location = icalEvent.Location.Replace("\\", "").Replace(",", ",").Trim(),
                            Name = icalEvent.Summary, // Use raw event name
                            TeamName = teamName
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