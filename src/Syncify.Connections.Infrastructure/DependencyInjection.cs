using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Google;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Connections.Infrastructure.Security;
using Syncify.Connections.Infrastructure.Services;

namespace Syncify.Connections.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddConnectionsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ConnectionsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICalendarAccountRepository, CalendarAccountRepository>();
        services.AddScoped<ITokenEncryptor, TokenEncryptor>();
        services.AddScoped<Syncify.Shared.IConnectionService, ConnectionService>();

        services.AddHttpClient<IOAuthProvider, GoogleOAuthProvider>();
        services.AddHttpClient<ICalendarProvider, GoogleCalendarProvider>();

        return services;
    }
}
