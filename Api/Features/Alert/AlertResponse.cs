namespace Api.Features.Alert;

public record AlertResponse(Guid PlotId, string Type, string Status, string Message, DateTime CreatedAt, DateTime? ResolvedAt);