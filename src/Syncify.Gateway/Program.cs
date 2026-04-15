using Syncify.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCorrelationId();
app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();