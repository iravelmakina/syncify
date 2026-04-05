using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Application.Commands.ResumeSyncRule;

public sealed class ResumeSyncRuleCommandHandler(
    ISyncRuleRepository repository,
    IConnectionService connectionService)
    : IRequestHandler<ResumeSyncRuleCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ResumeSyncRuleCommand request, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result<Unit>.Failure(new ApplicationError.NotFound("SyncRule", request.RuleId));

        var srcAccess = await connectionService.GetCalendarAccessAsync(rule.SourceCalendarId, ct);
        var tgtAccess = await connectionService.GetCalendarAccessAsync(rule.TargetCalendarId, ct);

        rule.Resume(srcAccess, tgtAccess, DateTime.UtcNow);
        await repository.UpdateAsync(rule, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}