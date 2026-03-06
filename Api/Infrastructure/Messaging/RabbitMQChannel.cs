using Api.Infrastructure.Settings;
using RabbitMQ.Client;

namespace Api.Infrastructure.Messaging;

public class RabbitMQChannel(RabbitMQSettings settings, ILogger<RabbitMQChannel> logger)
{
    public async Task<IModel> CreateAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                IConnection connection = factory.CreateConnection();

                IModel model = connection.CreateModel();

                model.ExchangeDeclare("sensor_events", ExchangeType.Topic, durable: true);

                model.QueueDeclare(
                    queue: "alert-service.sensor.readings",
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                model.QueueBind(
                    queue: "alert-service.sensor.readings",
                    exchange: "sensor_events",
                    routingKey: "sensor.data.ingested");

                model.BasicQos(0, prefetchCount: 10, global: false);

                logger.LogInformation("RabbitMQ conectado.");

                return model;
            }
            catch (Exception ex)
            {
                logger.LogWarning("RabbitMQ indisponível, tentando em 5s... {Msg}", ex.Message);

                await Task.Delay(5000, cancellationToken);
            }
        }

        throw new OperationCanceledException();
    }
}
