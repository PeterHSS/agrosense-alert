namespace Api.Domain.Events;

public record SensorDataIngestedEvent(Guid Id, Guid PlotId, double SoilMoisture, double Temperature, double Precipitation, DateTime Timestamp);
