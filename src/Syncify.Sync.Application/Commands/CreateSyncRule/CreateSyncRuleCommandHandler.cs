using MediatR;
using Syncify.Shared.Ports;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Sync.Application.Commands.CreateSyncRule;

public sealed class CreateSyncRuleCommandHandler(
    ISyncRuleRepository repository,
    IConnectionService connectionService)
    : IRequestHandler<CreateSyncRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSyncRuleCommand command, CancellationToken ct)
    {
        var srcAccess = await connectionService.GetCalendarAccessAsync(command.SourceCalendarId, ct);
        var tgtAccess = await connectionService.GetCalendarAccessAsync(command.TargetCalendarId, ct);

        var rule = SyncRule.Create(
            command.UserId,
            command.SourceCalendarId,
            command.TargetCalendarId,
            srcAccess,
            tgtAccess,
            command.CopyTitle,
            command.CustomTitle,
            command.FilterPolicy,
            DateTime.UtcNow);

        await repository.CreateAsync(rule, ct);
        return Result<Guid>.Success(rule.Id);
    }
}
