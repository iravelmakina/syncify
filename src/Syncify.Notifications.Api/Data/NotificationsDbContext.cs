using Microsoft.EntityFrameworkCore;

namespace Syncify.Notifications.Api.Data;

internal sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.Payload).HasColumnType("jsonb");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CorrelationId);
        });
    }
}