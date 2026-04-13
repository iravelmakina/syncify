using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Google;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Connections.Infrastructure.Security;
using Syncify.Connections.Infrastructure.Services;
using Syncify.Shared.Ports;

namespace Syncify.Connections.Infrastructure;

public static class DependencyInjection
{
    private static readonly TimeSpan GoogleTimeout = TimeSpan.FromSeconds(30);

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

        services.AddHttpClient<IOAuthProvider, GoogleOAuthProvider>((sp, client) =>
            ConfigureGoogleClient(
                client,
                sp.GetRequiredService<IOptions<GoogleOptions>>().Value.OAuthBaseUrl))
            .AddStandardResilienceHandler();

        services.AddHttpClient<ICalendarProvider, GoogleCalendarProvider>((sp, client) =>
            ConfigureGoogleClient(
                client,
                sp.GetRequiredService<IOptions<GoogleOptions>>().Value.ApiBaseUrl))
            .AddStandardResilienceHandler();

        return services;
    }

    public static async Task MigrateConnectionsDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConnectionsDbContext>();
        await db.Database.MigrateAsync();
    }

    private static void ConfigureGoogleClient(HttpClient client, string baseAddress)
    {
        client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        client.Timeout = GoogleTimeout;
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("syncify-connections", "1.0"));
    }
}
