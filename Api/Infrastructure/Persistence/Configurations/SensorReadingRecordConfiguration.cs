using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Persistence.Configurations;

public class SensorReadingRecordConfiguration : IEntityTypeConfiguration<SensorReadingRecord>
{
    public void Configure(EntityTypeBuilder<SensorReadingRecord> builder)
    {
        builder
            .ToTable("SensorReadingRecords");
        
        builder
            .HasKey(s => s.Id);
        
        builder
            .Property(s => s.PlotId)
            .IsRequired();
        
        builder
            .Property(s => s.Timestamp)
            .IsRequired();
    }
}
