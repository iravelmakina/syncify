using Scalar.AspNetCore;
using Syncify.Api.Behaviors;
using Syncify.Api.Endpoints;
using Syncify.Api.Middleware;
using Syncify.Api.OpenApi;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddOperationTransformer<UserIdHeaderTransformer>();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICalendarAccountRepository).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(ISyncRuleRepository).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(DomainExceptionBehavior<,>));
});

builder.Services.AddConnectionsModule(builder.Configuration);
builder.Services.AddSyncModule(builder.Configuration);

var app = builder.Build();

// 1. Run during development
// 2. Or if explicitly requested via --migrate flag (useful for CI/CD)
if (app.Environment.IsDevelopment() || args.Contains("--migrate"))
{
    await app.Services.MigrateConnectionsDatabaseAsync();
    await app.Services.MigrateSyncDatabaseAsync();
    
    if (args.Contains("--migrate"))
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Migrations completed successfully.");
        return;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();

app.UseMiddleware<UserIdMiddleware>();

app.MapConnectionEndpoints();
app.MapSyncRuleEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program;
