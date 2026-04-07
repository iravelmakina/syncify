using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Google;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Connections.Infrastructure.Security;
using Syncify.Connections.Infrastructure.Services;
using Syncify.Shared;

namespace Syncify.Connections.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddConnectionsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));

        services.AddDbContext<ConnectionsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICalendarAccountRepository, CalendarAccountRepository>();
        services.AddScoped<ITokenEncryptor, TokenEncryptor>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IConnectionsHealthCheck, ConnectionsHealthCheck>();

        services.AddHttpClient<IOAuthProvider, GoogleOAuthProvider>();
        services.AddHttpClient<ICalendarProvider, GoogleCalendarProvider>();

        return services;
    }

    public static async Task MigrateConnectionsDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConnectionsDbContext>();
        await db.Database.MigrateAsync();
    }
}
