using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.DTOs;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Application.Queries.GetSyncRule;

public sealed class GetSyncRuleQueryHandler(ISyncRuleRepository repository)
    : IRequestHandler<GetSyncRuleQuery, Result<SyncRuleResponse>>
{
    public async Task<Result<SyncRuleResponse>> Handle(GetSyncRuleQuery request, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result<SyncRuleResponse>.Failure(new ApplicationError.NotFound("SyncRule", request.RuleId));

        return Result<SyncRuleResponse>.Success(new SyncRuleResponse(
            rule.Id,
            rule.SourceCalendarId,
            rule.TargetCalendarId,
            rule.CopyTitle,
            rule.CustomTitle,
            rule.Status.ToString(),
            rule.CreatedAt));
    }
}