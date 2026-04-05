using Syncify.Api.Behaviors;
using Syncify.Api.Endpoints;
using Syncify.Api.Middleware;
using Syncify.Connections.Infrastructure;
using Syncify.Shared;
using Syncify.Sync.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Syncify.Connections.Application.Ports.ICalendarAccountRepository).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Syncify.Sync.Application.Ports.ISyncRuleRepository).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(DomainExceptionBehavior<,>));
});

builder.Services.AddConnectionsModule(builder.Configuration);
builder.Services.AddSyncModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseMiddleware<UserIdMiddleware>();

app.MapConnectionEndpoints();
app.MapSyncRuleEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program;