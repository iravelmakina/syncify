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
    IConnectionService connectionService,
    IPublishEndpoint publishEndpoint,
    ICorrelationIdAccessor correlationIdAccessor,
    IUnitOfWork unitOfWork,
    ISyncRuleRepository repository)
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

        repository.Add(rule);  // Add to DbContext without saving

        await publishEndpoint.Publish(new SyncRuleCreatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationIdAccessor.CorrelationId,
            SyncRuleId = rule.Id,
            UserId = command.UserId.Value,
            Summary = $"Sync rule created: {command.SourceCalendarId} → {command.TargetCalendarId}"
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);  // Single transaction: entity + outbox event

        return Result<Guid>.Success(rule.Id);
    }
}
