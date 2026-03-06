using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence.Contexts;

public sealed class AlertDbContext(DbContextOptions<AlertDbContext> options) : DbContext(options)
{
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<SensorReadingRecord> SensorReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlertDbContext).Assembly);
    }
}
