using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncify.Notifications.Api.Persistence;
using Syncify.Notifications.Api.Persistence.Entities;
using Syncify.Shared.Events;
using System.Text.Json;

namespace Syncify.Notifications.Api.Consumers;

internal sealed class SyncRuleCreatedConsumer : IConsumer<SyncRuleCreatedEvent>
{
    private readonly NotificationsDbContext _context;
    private readonly ILogger<SyncRuleCreatedConsumer> _logger;

    public SyncRuleCreatedConsumer(
        NotificationsDbContext context,
        ILogger<SyncRuleCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SyncRuleCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received SyncRuleCreated event: EventId={EventId}, SyncRuleId={SyncRuleId}, UserId={UserId}",
            message.EventId, message.SyncRuleId, message.UserId);

        var notification = CreateNotification(message);

        try
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Successfully stored notification for event {EventId}",
                message.EventId);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogWarning(
                "Duplicate event {EventId} ignored (already processed)",
                message.EventId);
        }
    }

    private Notification CreateNotification(SyncRuleCreatedEvent message)
    {
        return new Notification
        {
            EventId = message.EventId,
            EventType = "SyncRuleCreated",
            CorrelationId = message.CorrelationId,
            UserId = message.UserId,
            Summary = message.Summary,
            Payload = JsonSerializer.Serialize(message),
            OccurredAt = message.OccurredAt,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
    }
}
