using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Alert.ProcessReading;

public class ProcessReadingHandler(AlertDbContext context, ILogger<ProcessReadingHandler> logger)
{
    public async Task HandleAsync(SensorDataIngestedEvent @event, CancellationToken cancellationToken)
    {
        context.SensorReadings.Add(new SensorReadingRecord
        {
            PlotId = @event.PlotId,
            SoilMoisture = @event.SoilMoisture,
            Temperature = @event.Temperature,
            Precipitation = @event.Precipitation,
            Timestamp = @event.Timestamp
        });

        await context.SaveChangesAsync(cancellationToken);

        var since = DateTime.UtcNow.AddHours(-24);

        var last24h = await context.SensorReadings
            .Where(r => r.PlotId == @event.PlotId && r.Timestamp >= since)
            .ToListAsync(cancellationToken);

        var results = AlertRules.Evaluate(@event, last24h);

        foreach (var rule in results)
        {
            var existing = await context.Alerts.FirstOrDefaultAsync(alert => alert.PlotId == @event.PlotId && alert.Type == rule.Type && alert.Status == AlertStatus.Active, cancellationToken);

            if (rule.Triggered && existing is null)
            {
                context.Alerts.Add(new Domain.Entities.Alert
                {
                    PlotId = @event.PlotId,
                    Type = rule.Type,
                    Message = rule.Message
                });

                logger.LogWarning("[ALERTA ABERTO] {Type} | Talhão: {TalhaoId}", rule.Type, @event.PlotId);
            }
            else if (!rule.Triggered && existing is not null)
            {
                existing.Status = AlertStatus.Resolved;

                existing.ResolvedAt = DateTime.UtcNow;

                logger.LogInformation("[ALERTA RESOLVIDO] {Type} | Talhão: {TalhaoId}", rule.Type, @event.PlotId);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}