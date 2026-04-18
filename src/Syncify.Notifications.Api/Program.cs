using Syncify.Notifications.Api;
using Syncify.Notifications.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateNotificationsDatabaseAsync();
}

app.MapHealthEndpoints();

app.Run();