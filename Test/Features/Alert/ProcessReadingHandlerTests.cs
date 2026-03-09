using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Features.Alert.ProcessReading;

namespace Test.Features.Alert;

public class AlertRulesTests
{
    // ─── Helpers ──────────────────────────────────────────────────────────────────

    private static SensorDataIngestedEvent BuildEvent(double soilMoisture = 40.0, double temperature = 25.0, double precipitation = 10.0)
        => new(Guid.NewGuid(), Guid.NewGuid(), soilMoisture, temperature, precipitation, DateTime.UtcNow);

    private static SensorReadingRecord BuildReading(double soilMoisture = 40.0) =>
        new() { SoilMoisture = soilMoisture, Temperature = 25.0, Precipitation = 10.0, Timestamp = DateTime.UtcNow.AddHours(-1) };

    private static List<SensorReadingRecord> DryReadings(int count = 3)
        => [.. Enumerable.Range(0, count).Select(_ => BuildReading(soilMoisture: 20.0))];

    // ─── Evaluate returns all rules ───────────────────────────────────────────────

    [Fact]
    public void Evaluate_ReturnsThreeRules()
    {
        var results = AlertRules.Evaluate(BuildEvent(), []);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Evaluate_ReturnsOneResultPerAlertType()
    {
        var results = AlertRules.Evaluate(BuildEvent(), []);
        var types = results.Select(r => r.Type).ToList();

        Assert.Contains(AlertType.DroughtRisk, types);
        Assert.Contains(AlertType.HeatStress, types);
        Assert.Contains(AlertType.PestRisk, types);
    }

    // ─── DroughtRisk ──────────────────────────────────────────────────────────────

    [Fact]
    public void DroughtRisk_WhenSoilMoistureBelow30_AndAllLast24hBelow30_Triggers()
    {
        var @event = BuildEvent(soilMoisture: 20.0);
        var last24h = DryReadings();

        var result = AlertRules.Evaluate(@event, last24h).Single(r => r.Type == AlertType.DroughtRisk);

        Assert.True(result.Triggered);
    }

    [Fact]
    public void DroughtRisk_WhenSoilMoistureAbove30_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 35.0);
        var last24h = DryReadings();

        var result = AlertRules.Evaluate(@event, last24h).Single(r => r.Type == AlertType.DroughtRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void DroughtRisk_WhenSoilMoistureBelow30_ButNoHistoricalReadings_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 20.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.DroughtRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void DroughtRisk_WhenSoilMoistureBelow30_ButSomeLast24hAbove30_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 20.0);
        var last24h = new List<SensorReadingRecord>
        {
            BuildReading(soilMoisture: 20.0),
            BuildReading(soilMoisture: 45.0), // one healthy reading breaks the streak
        };

        var result = AlertRules.Evaluate(@event, last24h).Single(r => r.Type == AlertType.DroughtRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void DroughtRisk_WhenExactlyAt30_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 30.0);
        var last24h = DryReadings();

        var result = AlertRules.Evaluate(@event, last24h).Single(r => r.Type == AlertType.DroughtRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void DroughtRisk_MessageContainsSoilMoistureValue()
    {
        var @event = BuildEvent(soilMoisture: 20.0);

        var result = AlertRules.Evaluate(@event, DryReadings()).Single(r => r.Type == AlertType.DroughtRisk);

        Assert.Contains("20", result.Message);
    }

    // ─── HeatStress ───────────────────────────────────────────────────────────────

    [Fact]
    public void HeatStress_WhenTemperatureAbove38_Triggers()
    {
        var @event = BuildEvent(temperature: 39.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.HeatStress);

        Assert.True(result.Triggered);
    }

    [Fact]
    public void HeatStress_WhenTemperatureBelow38_DoesNotTrigger()
    {
        var @event = BuildEvent(temperature: 37.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.HeatStress);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void HeatStress_WhenTemperatureExactly38_DoesNotTrigger()
    {
        var @event = BuildEvent(temperature: 38.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.HeatStress);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void HeatStress_MessageContainsTemperatureValue()
    {
        var @event = BuildEvent(temperature: 40.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.HeatStress);

        Assert.Contains("40", result.Message);
    }

    // ─── PestRisk ─────────────────────────────────────────────────────────────────

    [Fact]
    public void PestRisk_WhenSoilMoistureAbove70_AndTemperatureBetween25And35_Triggers()
    {
        var @event = BuildEvent(soilMoisture: 80.0, temperature: 30.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.True(result.Triggered);
    }

    [Fact]
    public void PestRisk_WhenSoilMoistureBelow70_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 65.0, temperature: 30.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void PestRisk_WhenTemperatureBelow25_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 80.0, temperature: 24.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void PestRisk_WhenTemperatureAbove35_DoesNotTrigger()
    {
        var @event = BuildEvent(soilMoisture: 80.0, temperature: 36.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void PestRisk_WhenTemperatureExactly25_DoesNotTrigger()
    {
        // Range is > 25, so exactly 25 should not trigger
        var @event = BuildEvent(soilMoisture: 80.0, temperature: 25.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void PestRisk_WhenTemperatureExactly35_DoesNotTrigger()
    {
        // Range is < 35, so exactly 35 should not trigger
        var @event = BuildEvent(soilMoisture: 80.0, temperature: 35.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void PestRisk_WhenSoilMoistureExactly70_DoesNotTrigger()
    {
        // Condition is > 70, so exactly 70 should not trigger
        var @event = BuildEvent(soilMoisture: 70.0, temperature: 30.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.False(result.Triggered);
    }

    [Fact]
    public void PestRisk_MessageContainsSoilMoistureAndTemperature()
    {
        var @event = BuildEvent(soilMoisture: 75.0, temperature: 30.0);

        var result = AlertRules.Evaluate(@event, []).Single(r => r.Type == AlertType.PestRisk);

        Assert.Contains("75", result.Message);
        Assert.Contains("30", result.Message);
    }
}