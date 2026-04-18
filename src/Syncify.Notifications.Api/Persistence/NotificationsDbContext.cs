using Microsoft.EntityFrameworkCore;
using Syncify.Notifications.Api.Persistence.Entities;

namespace Syncify.Notifications.Api.Persistence;

internal sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}