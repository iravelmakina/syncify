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
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Summary).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Payload).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.OccurredAt);
        });
    }
}