using Api.Domain.Entities;
using Api.Domain.Events;
using Api.Infrastructure.Persistence.Contexts;

namespace Api.Domain.Abstractions.Rules;

public interface IAlertRule
{
    AlertType AlertType { get; }

    Task<bool> IsTriggeredAsync(SensorDataIngestedEvent @event, AlertDbContext context, CancellationToken cancellationToken = default);

    string BuildMessage(SensorDataIngestedEvent @event);
}
