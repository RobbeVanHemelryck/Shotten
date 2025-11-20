
using BE.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.Data;

public class ShottenContext : DbContext
{
    public ShottenContext(DbContextOptions<ShottenContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Team> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>()
            .HasKey(a => new { a.MatchId, a.PlayerId });

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Match)
            .WithMany(m => m.Attendances)
            .HasForeignKey(a => a.MatchId);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Player)
            .WithMany()
            .HasForeignKey(a => a.PlayerId);

        modelBuilder.Entity<Player>()
            .HasMany(p => p.Teams)
            .WithMany(t => t.Players);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.Team)
            .WithMany(t => t.Matches)
            .HasForeignKey(m => m.TeamId);
    }
}