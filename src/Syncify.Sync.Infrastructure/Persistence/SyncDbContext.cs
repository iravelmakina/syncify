using Microsoft.EntityFrameworkCore;
using Syncify.Sync.Infrastructure.Persistence.Entities;

namespace Syncify.Sync.Infrastructure.Persistence;

internal sealed class SyncDbContext : DbContext
{
    public DbSet<SyncRuleEntity> SyncRules => Set<SyncRuleEntity>();
    public DbSet<SyncedEventEntity> SyncedEvents => Set<SyncedEventEntity>();

    public SyncDbContext(DbContextOptions<SyncDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SyncDbContext).Assembly);
    }
}
