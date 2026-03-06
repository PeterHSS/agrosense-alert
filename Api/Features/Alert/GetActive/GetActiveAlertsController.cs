using Api.Common;
using Api.Features.Alert.Get;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Alert.GetActive;

[ApiController]
[Route("api/alerts/active")]
[Authorize(Policy = Policies.UserOnly)]
public class GetActiveAlertsController : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetActiveAlerts([FromServices] GetActiveAlertsUseCase useCase)
    {
        var result = await useCase.Handle();

        if (result.IsFailure)
            return Results.BadRequest();
        
        return Results.Ok(result.Value);
    }
}
