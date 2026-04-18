using MediatR;
using Syncify.Shared.Errors;
using Syncify.Shared.Ports;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Application.Commands.UpdateTitle;

public sealed class UpdateTitleCommandHandler(
    ISyncRuleRepository repository,
    IConnectionService connectionService)
    : IRequestHandler<UpdateTitleCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateTitleCommand request, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result<Unit>.Failure(new ApplicationError.NotFound("SyncRule", request.RuleId));

        var srcAccess = await connectionService.GetCalendarAccessAsync(rule.SourceCalendarId, rule.UserId, ct);

        rule.UpdateTitle(request.CopyTitle, request.CustomTitle, srcAccess, DateTime.UtcNow);
        await repository.UpdateAsync(rule, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
