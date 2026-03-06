using Api.Common;
using Api.Domain.Entities;
using Api.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Alert.Get;

public class GetActiveAlertsUseCase(AlertDbContext context) 
{
    public async Task<Result<IEnumerable<AlertResponse>>> Handle()
    {
        var alerts = await context.Alerts
            .Where(a => a.Status == AlertStatus.Active)
            .Select(a => new AlertResponse(a.PlotId, a.Type.ToString().ToUpper(), a.Status.ToString().ToUpper(), a.Message, a.CreatedAt, a.ResolvedAt))
            .ToListAsync();

        return Result<IEnumerable<AlertResponse>>.Success(alerts);
    }
}
