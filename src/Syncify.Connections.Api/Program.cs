using MediatR;
using Scalar.AspNetCore;
using Syncify.Connections.Api.Endpoints;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure;
using Syncify.Shared.Behaviors;
using Syncify.Shared.Middleware;
using Syncify.Shared.OpenApi;

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
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(DomainExceptionBehavior<,>));
});

builder.Services.AddConnectionsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || args.Contains("--migrate"))
{
    await app.Services.MigrateConnectionsDatabaseAsync();

    if (args.Contains("--migrate"))
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Connections migrations completed successfully.");
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
app.MapInternalConnectionEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program;