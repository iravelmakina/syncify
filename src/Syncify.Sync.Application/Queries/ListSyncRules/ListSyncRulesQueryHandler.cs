using MediatR;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Sync.Application.Queries.ListSyncRules;

public sealed class ListSyncRulesQueryHandler(ISyncRuleRepository repository)
    : IRequestHandler<ListSyncRulesQuery, Result<IReadOnlyList<SyncRule>>>
{
    public async Task<Result<IReadOnlyList<SyncRule>>> Handle(ListSyncRulesQuery request, CancellationToken ct)
    {
        var rules = await repository.ListByUserAsync(request.UserId, ct);
        return Result<IReadOnlyList<SyncRule>>.Success(rules);
    }
}
