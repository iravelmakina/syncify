using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syncify.Sync.Infrastructure.Persistence.Entities;

namespace Syncify.Sync.Infrastructure.Persistence.Configurations;

internal sealed class SyncedEventConfiguration : IEntityTypeConfiguration<SyncedEventEntity>
{
    public void Configure(EntityTypeBuilder<SyncedEventEntity> builder)
    {
        builder.ToTable("synced_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.SyncRuleId)
            .HasColumnName("sync_rule_id")
            .IsRequired();

        builder.Property(x => x.SourceEventId)
            .HasColumnName("source_event_id")
            .IsRequired();

        builder.Property(x => x.TargetBlockId)
            .HasColumnName("target_block_id")
            .IsRequired();

        builder.Property(x => x.SourceUpdatedAt)
            .HasColumnName("source_updated_at")
            .IsRequired();

        builder.HasOne(x => x.SyncRule)
            .WithMany()
            .HasForeignKey(x => x.SyncRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for lookups
        builder.HasIndex(x => new { x.SyncRuleId, x.SourceEventId }).IsUnique();
    }
}
