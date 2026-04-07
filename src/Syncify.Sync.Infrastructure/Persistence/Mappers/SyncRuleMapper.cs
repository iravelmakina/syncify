using System.Text.Json;
using Syncify.Shared;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.Enums;
using Syncify.Sync.Domain.ValueObjects;
using Syncify.Sync.Infrastructure.Persistence.Entities;

namespace Syncify.Sync.Infrastructure.Persistence.Mappers;

public static class SyncRuleMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static SyncRule ToDomain(this SyncRuleEntity entity)
    {
        return SyncRule.Reconstitute(
            entity.Id,
            new UserId(entity.UserId),
            entity.SourceCalendarId,
            entity.TargetCalendarId,
            entity.CopyTitle,
            entity.CustomTitle,
            JsonSerializer.Deserialize<FilterPolicy>(entity.FilterPolicyJson, JsonOptions)!,
            Enum.Parse<SyncRuleStatus>(entity.Status),
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
            FilterPolicyJson = JsonSerializer.Serialize(rule.FilterPolicy, JsonOptions),
            Status = rule.Status.ToString(),
            SyncCursor = rule.SyncCursor,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
