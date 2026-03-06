namespace Api.Infrastructure.Settings;

public class RabbitMQSettings
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
