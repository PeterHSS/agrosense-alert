using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Persistence.Configurations;

internal sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder
            .ToTable("Alerts");

        builder
            .HasKey(a => a.Id);

        builder
            .Property(a => a.PlotId)
            .IsRequired();
        
        builder
            .Property(a => a.Type)
            .HasConversion<string>()
            .IsRequired();
        
        builder
            .Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();
        
        builder
            .Property(a => a.Message)
            .HasMaxLength(500)
            .IsRequired();
        
        builder
            .Property(a => a.CreatedAt)
            .IsRequired();
        
        builder
            .Property(a => a.ResolvedAt);
    }
}
