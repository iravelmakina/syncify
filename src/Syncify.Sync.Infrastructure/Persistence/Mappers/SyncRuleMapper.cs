using Syncify.Shared;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.Enums;
using Syncify.Sync.Infrastructure.Persistence.Entities;

namespace Syncify.Sync.Infrastructure.Persistence.Mappers;

internal static class SyncRuleMapper
{
    public static SyncRule ToDomain(this SyncRuleEntity entity)
    {
        return SyncRule.Reconstitute(
            entity.Id,
            new UserId(entity.UserId),
            entity.SourceCalendarId,
            entity.TargetCalendarId,
            entity.CopyTitle,
            entity.CustomTitle,
            StoredFilterPolicyMapper.Deserialize(entity.FilterPolicyJson),
            Enum.Parse<SyncRuleStatus>(entity.Status, ignoreCase: true),
            entity.SyncCursor,
            entity.CreatedAt.UtcDateTime,
            entity.UpdatedAt.UtcDateTime);
    }

    public static SyncRuleEntity ToEntity(this SyncRule rule)
    {
        return new SyncRuleEntity
        {
            Id = rule.Id,
            UserId = rule.UserId.Value,
            SourceCalendarId = rule.SourceCalendarId,
            TargetCalendarId = rule.TargetCalendarId,
            CopyTitle = rule.CopyTitle,
            CustomTitle = rule.CustomTitle,
            FilterPolicyJson = StoredFilterPolicyMapper.Serialize(rule.FilterPolicy),
            Status = rule.Status.ToString(),
            SyncCursor = rule.SyncCursor,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
