using Syncify.Sync.Application.DTOs;
using Syncify.Sync.Infrastructure.Persistence.Entities;

namespace Syncify.Sync.Infrastructure.Persistence.Mappers;

public static class SyncedEventMapper
{
    public static SyncedEventMapping ToDto(this SyncedEventEntity entity)
    {
        return new SyncedEventMapping
        {
            Id = entity.Id,
            SyncRuleId = entity.SyncRuleId,
            SourceEventId = entity.SourceEventId,
            TargetBlockId = entity.TargetBlockId,
            SourceUpdatedAt = entity.SourceUpdatedAt
        };
    }

    public static SyncedEventEntity ToEntity(this SyncedEventMapping mapping)
    {
        return new SyncedEventEntity
        {
            Id = mapping.Id,
            SyncRuleId = mapping.SyncRuleId,
            SourceEventId = mapping.SourceEventId,
            TargetBlockId = mapping.TargetBlockId,
            SourceUpdatedAt = mapping.SourceUpdatedAt
        };
    }
}
