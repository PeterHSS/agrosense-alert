using Api;
using Api.Common.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependecyInjection(builder.Configuration);

Serilog.Debugging.SelfLog.Enable(Console.Error);

builder.Host.AddSerilog();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

builder.Services
    .AddOpenTelemetry()
    .WithMetrics(metrics =>
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AgroSense.Alert.Api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddNpgsqlInstrumentation()
            .AddPrometheusExporter())
    .WithTracing(tracing =>
        tracing
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddNpgsql());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.Services.ApplyMigrations();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint("/api/metrics/alerts");

app.UseMiddleware<RequestLogContextMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.UseHealthChecks("/health", new HealthCheckOptions { ResponseWriter = async (context, report) => { context.Response.ContentType = "text/plain"; await context.Response.WriteAsync("OK"); } });

app.UseSerilogRequestLogging();

app.Run();
