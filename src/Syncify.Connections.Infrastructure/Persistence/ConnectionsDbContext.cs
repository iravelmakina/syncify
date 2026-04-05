using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Infrastructure.Persistence.Entities;

namespace Syncify.Connections.Infrastructure.Persistence;

public class ConnectionsDbContext : DbContext
{
    public DbSet<CalendarAccountEntity> CalendarAccounts => Set<CalendarAccountEntity>();
    public DbSet<CalendarEntity> Calendars => Set<CalendarEntity>();

    public ConnectionsDbContext(DbContextOptions<ConnectionsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConnectionsDbContext).Assembly);
    }
}
