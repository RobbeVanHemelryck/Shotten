using BE.Models;
using BE.Services;
using Microsoft.EntityFrameworkCore;

namespace BE.Data;

public static class SeedData
{
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

            // Seed Matches from iCal using the service
            if (!context.Matches.Any())
            {
                var matchSyncService = serviceProvider.GetRequiredService<MatchSyncService>();
                await matchSyncService.SyncMatchesAsync();
            }
        }
    }
}