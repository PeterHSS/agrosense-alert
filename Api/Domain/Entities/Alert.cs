namespace Api.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlotId { get; set; }
    public AlertType Type { get; set; }
    public AlertStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
