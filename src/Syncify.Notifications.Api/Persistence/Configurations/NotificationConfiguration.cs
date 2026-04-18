using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syncify.Notifications.Api.Persistence.Entities;

namespace Syncify.Notifications.Api.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CorrelationId)
            .HasColumnName("correlation_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.Summary)
            .HasColumnName("summary")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.IsRead)
            .HasColumnName("is_read")
            .IsRequired();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_notifications_user_id");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("ix_notifications_event_type");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("ix_notifications_correlation_id");

        builder.HasIndex(e => new { e.UserId, e.IsRead })
            .HasDatabaseName("ix_notifications_user_id_is_read");

        builder.HasIndex(e => e.OccurredAt)
            .HasDatabaseName("ix_notifications_occurred_at");
    }
}
