
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
    }
}