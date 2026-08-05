using Microsoft.EntityFrameworkCore;
using Monitoring.Api.Models;

namespace Monitoring.Api.Data;

/// <summary>
/// Entity Framework Core database context for the monitoring application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MonitoringTarget> MonitoringTargets { get; set; }
    public DbSet<MonitoringResult> MonitoringResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MonitoringTarget
        modelBuilder.Entity<MonitoringTarget>()
            .HasKey(mt => mt.Id);

        modelBuilder.Entity<MonitoringTarget>()
            .HasMany(mt => mt.Results)
            .WithOne(mr => mr.MonitoringTarget)
            .HasForeignKey(mr => mr.MonitoringTargetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure MonitoringResult
        modelBuilder.Entity<MonitoringResult>()
            .HasKey(mr => mr.Id);

        modelBuilder.Entity<MonitoringResult>()
            .HasIndex(mr => mr.MonitoringTargetId);

        modelBuilder.Entity<MonitoringResult>()
            .HasIndex(mr => mr.CheckedAt);

        // Seed initial data (optional)
        modelBuilder.Entity<MonitoringTarget>().HasData(
            new MonitoringTarget
            {
                Id = 1,
                Name = "Google",
                Url = "https://www.google.com",
                MonitoringInterval = 60,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
