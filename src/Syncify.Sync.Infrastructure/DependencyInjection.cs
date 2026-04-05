using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Infrastructure.Persistence;

namespace Syncify.Sync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSyncModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SyncDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISyncRuleRepository, SyncRuleRepository>();
        services.AddScoped<ISyncedEventRepository, SyncedEventRepository>();

        // GoogleCalendarSyncer and SyncPoller will be added here later

        return services;
    }
}
