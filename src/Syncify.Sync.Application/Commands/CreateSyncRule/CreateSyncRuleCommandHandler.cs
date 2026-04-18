using MassTransit;
using Syncify.Shared.Correlation;
using Syncify.Shared.Events;
using MediatR;
using Syncify.Shared.Ports;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Sync.Application.Commands.CreateSyncRule;

public sealed class CreateSyncRuleCommandHandler(
    ISyncRuleRepository repository,
    IConnectionService connectionService,
    IPublishEndpoint publishEndpoint,
    ICorrelationIdAccessor correlationIdAccessor)
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
            command.LookbackDays,
            DateTime.UtcNow);

        await repository.CreateAsync(rule, ct);

        await publishEndpoint.Publish(new SyncRuleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationIdAccessor.CorrelationId,
            SyncRuleId = rule.Id,
            UserId = command.UserId.Value,
            Summary = $"Sync rule created: {command.SourceCalendarId} → {command.TargetCalendarId}"
        }, ct);

        return Result<Guid>.Success(rule.Id);
    }
}
