using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syncify.Sync.Infrastructure.Persistence.Entities;

namespace Syncify.Sync.Infrastructure.Persistence.Configurations;

internal sealed class SyncRuleConfiguration : IEntityTypeConfiguration<SyncRuleEntity>
{
    public void Configure(EntityTypeBuilder<SyncRuleEntity> builder)
    {
        builder.ToTable("sync_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.SourceCalendarId)
            .HasColumnName("source_calendar_id")
            .IsRequired();

        builder.Property(x => x.TargetCalendarId)
            .HasColumnName("target_calendar_id")
            .IsRequired();

        builder.Property(x => x.CopyTitle)
            .HasColumnName("copy_title")
            .IsRequired();

        builder.Property(x => x.CustomTitle)
            .HasColumnName("custom_title")
            .IsRequired();

        builder.Property(x => x.FilterPolicyJson)
            .HasColumnName("filter_policy")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.LookbackDays)
            .HasColumnName("lookback_days")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.SyncCursor)
            .HasColumnName("sync_cursor");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
