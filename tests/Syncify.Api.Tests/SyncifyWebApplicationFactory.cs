using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Sync.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Syncify.Api.Tests;

public sealed class SyncifyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceDbContext<ConnectionsDbContext>(services);
            ReplaceDbContext<SyncDbContext>(services);
        });
    }

    private void ReplaceDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<TContext>));

        if (descriptor is not null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(_postgres.GetConnectionString()));
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var connectionsDb = scope.ServiceProvider.GetRequiredService<ConnectionsDbContext>();
        var syncDb = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
        await connectionsDb.Database.EnsureCreatedAsync();
        await syncDb.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}