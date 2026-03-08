using System.Diagnostics.Metrics;
using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Alert.ProcessReading;

public class ProcessReadingHandler(AlertDbContext context, ILogger<ProcessReadingHandler> logger)
{
    private static readonly Meter _meter = new("AgroSense.Alert");

    private static double _lastSoilMoisture;
    private static double _lastTemperature;
    private static double _lastPrecipitation;


    private static readonly ObservableGauge<double> _soilMoistureGauge = _meter.CreateObservableGauge("agrosense_soil_moisture_percent", () => _lastSoilMoisture);
    private static readonly ObservableGauge<double> _temperatureGauge = _meter.CreateObservableGauge("agrosense_temperature_celsius", () => _lastTemperature);
    private static readonly ObservableGauge<double> _precipitationGauge = _meter.CreateObservableGauge("agrosense_precipitation_mm", () => _lastPrecipitation);
    private static readonly Counter<long> _alertsOpened = _meter.CreateCounter<long>("agrosense_alerts_opened_total");
    private static readonly Counter<long> _alertsResolved = _meter.CreateCounter<long>("agrosense_alerts_resolved_total");

    public async Task HandleAsync(SensorDataIngestedEvent @event, CancellationToken cancellationToken)
    {
        _lastSoilMoisture = @event.SoilMoisture;
        _lastTemperature = @event.Temperature;
        _lastPrecipitation = @event.Precipitation;

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

        logger.LogInformation("[PROCESSANDO LEITURA] Talhão: {PlotId} | Umidade: {SoilMoisture} | Temp: {Temperature} | Chuva: {Precipitation}",
            @event.PlotId, @event.SoilMoisture, @event.Temperature, @event.Precipitation);

        foreach (var rule in results)
        {
            logger.LogInformation("[REGRA AVALIADA] {Type} | Triggered: {Triggered}", rule.Type, rule.Triggered);

            var existing = await context.Alerts.FirstOrDefaultAsync(alert => alert.PlotId == @event.PlotId && alert.Type == rule.Type && alert.Status == AlertStatus.Active, cancellationToken);

            if (rule.Triggered && existing is null)
            {
                context.Alerts.Add(new Domain.Entities.Alert
                {
                    PlotId = @event.PlotId,
                    Type = rule.Type,
                    Message = rule.Message

                });

                _alertsOpened.Add(1, new KeyValuePair<string, object?>("alert_type", rule.Type.ToString()), new KeyValuePair<string, object?>("plot_id", @event.PlotId.ToString()));

                logger.LogWarning("[ALERTA ABERTO] {Type} | Talhão: {TalhaoId}", rule.Type, @event.PlotId);
            }
            else if (!rule.Triggered && existing is not null)
            {
                existing.Status = AlertStatus.Resolved;

                existing.ResolvedAt = DateTime.UtcNow;

                _alertsResolved.Add(1, new KeyValuePair<string, object?>("alert_type", rule.Type.ToString()), new KeyValuePair<string, object?>("plot_id", @event.PlotId.ToString()));

                logger.LogInformation("[ALERTA RESOLVIDO] {Type} | Talhão: {TalhaoId}", rule.Type, @event.PlotId);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}