using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Persistence.Entities;

namespace Syncify.Connections.Infrastructure.Persistence;

internal sealed class ConnectionsDbContext : DbContext
{
    private readonly ITokenEncryptor _encryptor;

    public DbSet<CalendarAccountEntity> CalendarAccounts => Set<CalendarAccountEntity>();
    public DbSet<CalendarEntity> Calendars => Set<CalendarEntity>();

    public ConnectionsDbContext(DbContextOptions<ConnectionsDbContext> options, ITokenEncryptor encryptor)
        : base(options)
    {
        _encryptor = encryptor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConnectionsDbContext).Assembly);

        modelBuilder.Entity<CalendarAccountEntity>()
            .Property(x => x.RefreshToken)
            .HasConversion(
                v => _encryptor.Encrypt(v),
                v => _encryptor.Decrypt(v));
    }
}
