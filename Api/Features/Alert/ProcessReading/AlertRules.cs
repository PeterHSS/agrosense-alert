using Api.Domain.Entities;
using Api.Domain.Events;

namespace Api.Features.Alert.ProcessReading;

public record RuleResult(AlertType Type, bool Triggered, string Message);

public static class AlertRules
{
    public static List<RuleResult> Evaluate(
        SensorDataIngestedEvent reading,
        IEnumerable<SensorReadingRecord> last24h)
    {
        return
        [
            Drought(reading, last24h),
            Heat(reading),
            PestRisk(reading),
        ];
    }

    private static RuleResult Drought(SensorDataIngestedEvent @event, IEnumerable<SensorReadingRecord> last24h)
    {
        var triggered = @event.SoilMoisture < 30 && last24h.Any() && last24h.All(x => x.SoilMoisture < 30);

        return new(AlertType.DroughtRisk, triggered, $"Alerta de Seca: umidade em {@event.SoilMoisture:F1}% abaixo de 30% por mais de 24h.");
    }

    private static RuleResult Heat(SensorDataIngestedEvent @event) =>
        new(AlertType.HeatStress, @event.Temperature > 38, $"Estresse Térmico: temperatura em {@event.Temperature:F1}°C acima de 38°C.");
    private static RuleResult PestRisk(SensorDataIngestedEvent @event) =>
        new(AlertType.PestRisk, @event.SoilMoisture > 70 && @event.Temperature is > 25 and < 35, $"Risco de Praga: umidade {@event.SoilMoisture:F1}% e temperatura {@event.Temperature:F1}°C favoráveis.");
}