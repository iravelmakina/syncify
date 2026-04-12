using Syncify.Api.Responses;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Api.Mappers;

internal static class SyncRuleMapper
{
    public static SyncRuleResponse ToResponse(this SyncRule rule) =>
        new(rule.Id, rule.SourceCalendarId, rule.TargetCalendarId,
            rule.CopyTitle, rule.CustomTitle, rule.Status.ToString(), rule.CreatedAt);

    public static IReadOnlyList<SyncRuleResponse> ToResponse(this IReadOnlyList<SyncRule> rules) =>
        rules.Select(ToResponse).ToList();
}