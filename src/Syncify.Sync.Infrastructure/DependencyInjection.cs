using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Application.Services;
using Syncify.Sync.Infrastructure.Google;
using Syncify.Sync.Infrastructure.Persistence;
using Syncify.Sync.Infrastructure.Polling;

namespace Syncify.Sync.Infrastructure;

public static class DependencyInjection
{
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

        services.AddHttpClient<ICalendarSyncer, GoogleCalendarSyncer>();
        services.AddHostedService<SyncPoller>();

        return services;
    }
}
