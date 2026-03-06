using System.Text;
using System.Text.Json;
using Api.Domain.Events;
using Api.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Api.Features.Alert.ProcessReading;

public class ProcessReadingConsumer(IServiceScopeFactory scopeFactory, RabbitMQChannel rabbitMQChannel, ILogger<ProcessReadingConsumer> logger) : BackgroundService
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var channel = await rabbitMQChannel.CreateAsync(cancellationToken);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (_, args) =>
        {
            var body = Encoding.UTF8.GetString(args.Body.ToArray());

            var reading = JsonSerializer.Deserialize<SensorDataIngestedEvent>(body, _jsonOptions);

            if (reading is null)
            {
                channel.BasicNack(args.DeliveryTag, false, requeue: false);

                return;
            }

            try
            {
                using var scope = scopeFactory.CreateScope();

                var handler = scope.ServiceProvider.GetRequiredService<ProcessReadingHandler>();


                await handler.HandleAsync(reading, cancellationToken);

                channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar leitura {PlotId}", reading.PlotId);

                channel.BasicNack(args.DeliveryTag, false, requeue: true);
            }
        };

        channel.BasicConsume("alert-service.sensor.readings", autoAck: false, consumer);

        logger.LogInformation("Consumindo fila: alert-service.sensor.readings");

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}