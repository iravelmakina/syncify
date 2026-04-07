using MediatR;
using Syncify.Shared.Errors;
using Syncify.Shared.Ports;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Application.Commands.UpdateFilter;

public sealed class UpdateFilterCommandHandler(
    ISyncRuleRepository repository,
    IConnectionService connectionService)
    : IRequestHandler<UpdateFilterCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateFilterCommand request, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result<Unit>.Failure(new ApplicationError.NotFound("SyncRule", request.RuleId));

        var srcAccess = await connectionService.GetCalendarAccessAsync(rule.SourceCalendarId, ct);

        rule.UpdateFilter(request.FilterPolicy, srcAccess, DateTime.UtcNow);
        await repository.UpdateAsync(rule, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
