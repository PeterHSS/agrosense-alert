using Api.Domain.Entities;
using Api.Features.Alert.GetActive;
using Api.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Test.Features.Alert;

public class GetActiveAlertsUseCaseTests
{
    private readonly AlertDbContext _context;
    private readonly GetActiveAlertsUseCase _useCase;

    public GetActiveAlertsUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<AlertDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AlertDbContext(options);
        _useCase = new GetActiveAlertsUseCase(_context);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<Api.Domain.Entities.Alert> SeedAlertAsync(AlertStatus status, AlertType type = AlertType.HeatStress)
    {
        var alert = new Api.Domain.Entities.Alert
        {
            PlotId = Guid.NewGuid(),
            Type = type,
            Message = "Mensagem de teste",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            ResolvedAt = status == AlertStatus.Resolved ? DateTime.UtcNow : null
        };
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    // ─── Always succeeds ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AlwaysReturnsSuccess()
    {
        var result = await _useCase.Handle();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenNoAlerts_ReturnsEmptyList()
    {
        var result = await _useCase.Handle();

        Assert.Empty(result.Value);
    }

    // ─── Filtering ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnsOnlyActiveAlerts()
    {
        await SeedAlertAsync(AlertStatus.Active);
        await SeedAlertAsync(AlertStatus.Resolved);

        var result = await _useCase.Handle();

        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Handle_WhenOnlyResolvedAlerts_ReturnsEmptyList()
    {
        await SeedAlertAsync(AlertStatus.Resolved);
        await SeedAlertAsync(AlertStatus.Resolved);

        var result = await _useCase.Handle();

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsAllActiveAlerts_WhenMultipleExist()
    {
        await SeedAlertAsync(AlertStatus.Active, AlertType.HeatStress);
        await SeedAlertAsync(AlertStatus.Active, AlertType.DroughtRisk);
        await SeedAlertAsync(AlertStatus.Active, AlertType.PestRisk);
        await SeedAlertAsync(AlertStatus.Resolved);

        var result = await _useCase.Handle();

        Assert.Equal(3, result.Value.Count());
    }

    // ─── Mapping ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_MapsFieldsCorrectly()
    {
        var alert = await SeedAlertAsync(AlertStatus.Active, AlertType.HeatStress);

        var result = await _useCase.Handle();

        var response = result.Value.Single();
        Assert.Equal(alert.PlotId, response.PlotId);
        Assert.Equal(alert.Message, response.Message);
        Assert.Equal(alert.CreatedAt, response.CreatedAt);
        Assert.Null(response.ResolvedAt);
    }

    [Fact]
    public async Task Handle_MapsTypeToUpperCaseString()
    {
        await SeedAlertAsync(AlertStatus.Active, AlertType.HeatStress);

        var result = await _useCase.Handle();

        Assert.Equal("HEATSTRESS", result.Value.Single().Type);
    }

    [Fact]
    public async Task Handle_MapsStatusToUpperCaseString()
    {
        await SeedAlertAsync(AlertStatus.Active);

        var result = await _useCase.Handle();

        Assert.Equal("ACTIVE", result.Value.Single().Status);
    }
}