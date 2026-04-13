using MediatR;
using Syncify.Shared.Errors;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Sync.Application.Queries.GetSyncRule;

public sealed class GetSyncRuleQueryHandler(ISyncRuleRepository repository)
    : IRequestHandler<GetSyncRuleQuery, Result<SyncRule>>
{
    public async Task<Result<SyncRule>> Handle(GetSyncRuleQuery request, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result<SyncRule>.Failure(new ApplicationError.NotFound("SyncRule", request.RuleId));

        return Result<SyncRule>.Success(rule);
    }
}
