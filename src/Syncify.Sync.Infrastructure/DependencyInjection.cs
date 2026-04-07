using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Application.Services;
using Syncify.Sync.Infrastructure.Google;
using Syncify.Sync.Infrastructure.Persistence;
using Syncify.Sync.Infrastructure.Polling;

namespace Syncify.Sync.Infrastructure;

public static class DependencyInjection
{
    private static readonly TimeSpan GoogleTimeout = TimeSpan.FromSeconds(30);

    public static IServiceCollection AddSyncModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleSyncOptions>(configuration.GetSection(GoogleSyncOptions.SectionName));
        services.Configure<SyncPollerOptions>(configuration.GetSection(SyncPollerOptions.SectionName));

        services.AddDbContext<SyncDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISyncRuleRepository, SyncRuleRepository>();
        services.AddScoped<ISyncedEventRepository, SyncedEventRepository>();
        services.AddScoped<ISyncHealthCheck, SyncHealthCheck>();
        services.AddScoped<SyncExecutor>();

        services.AddHttpClient<ICalendarSyncer, GoogleCalendarSyncer>((sp, client) =>
            ConfigureGoogleClient(
                client,
                sp.GetRequiredService<IOptions<GoogleSyncOptions>>().Value.ApiBaseUrl))
            .AddStandardResilienceHandler();
        services.AddHostedService<SyncPoller>();

        return services;
    }

    public static async Task MigrateSyncDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
        await db.Database.MigrateAsync();
    }

    private static void ConfigureGoogleClient(HttpClient client, string baseAddress)
    {
        client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        client.Timeout = GoogleTimeout;
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("syncify-sync", "1.0"));
    }
}
