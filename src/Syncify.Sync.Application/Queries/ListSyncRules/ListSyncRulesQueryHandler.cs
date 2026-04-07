using MediatR;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Application.Responses;

namespace Syncify.Sync.Application.Queries.ListSyncRules;

public sealed class ListSyncRulesQueryHandler(ISyncRuleRepository repository)
    : IRequestHandler<ListSyncRulesQuery, Result<IReadOnlyList<SyncRuleResponse>>>
{
    public async Task<Result<IReadOnlyList<SyncRuleResponse>>> Handle(ListSyncRulesQuery request, CancellationToken ct)
    {
        var rules = await repository.ListByUserAsync(request.UserId, ct);

        var response = rules.Select(r => new SyncRuleResponse(
            r.Id,
            r.SourceCalendarId,
            r.TargetCalendarId,
            r.CopyTitle,
            r.CustomTitle,
            r.Status.ToString(),
            r.CreatedAt)).ToList();

        return Result<IReadOnlyList<SyncRuleResponse>>.Success(response);
    }
}
