using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Syncify.Shared.Ports;
using Syncify.Sync.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Syncify.Sync.Api.Tests;

public sealed class SyncifyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
        .Build();

    public Mock<IConnectionService> ConnectionServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceDbContext<SyncDbContext>(services);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionService));
            if (descriptor != null) services.Remove(descriptor);
            services.AddScoped(_ => ConnectionServiceMock.Object);
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
        var syncDb = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
        await syncDb.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
