using MassTransit;
using Moq;
using Syncify.Shared;
using Syncify.Shared.Correlation;
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;
using Syncify.Shared.Events;
using Syncify.Shared.Ports;
using Syncify.Sync.Application.Commands.CreateSyncRule;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Tests;

public class CreateSyncRuleTests
{
    private readonly Mock<ISyncRuleRepository> _repositoryMock = new();
    private readonly Mock<IConnectionService> _connectionServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock = new();
    private readonly CreateSyncRuleCommandHandler _handler;

    public CreateSyncRuleTests()
    {
        _handler = new CreateSyncRuleCommandHandler(
            _repositoryMock.Object,
            _connectionServiceMock.Object,
            _publishEndpointMock.Object,
            _correlationIdAccessorMock.Object);
    }

    [Fact]
    public async Task CreateSyncRuleUseCase_ValidAccess_CreatesActiveRule_AndPublishesEvent()
    {
        // Arrange
        var userId = UserId.New();
        var srcId = Guid.NewGuid();
        var tgtId = Guid.NewGuid();
        const string correlationId = "corr-123";
        var command = new CreateSyncRuleCommand(
            userId,
            srcId,
            tgtId,
            false,
            "Busy",
            new FilterPolicy([]));

        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(srcId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.Read);
        
        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(tgtId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.ReadWrite);

        _correlationIdAccessorMock
            .SetupGet(x => x.CorrelationId)
            .Returns(correlationId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.CreateAsync(
            It.Is<SyncRule>(r =>
                r.UserId == userId &&
                r.SourceCalendarId == srcId &&
                r.TargetCalendarId == tgtId),
            It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<SyncRuleCreatedEvent>(e =>
                e.CorrelationId == correlationId &&
                e.SyncRuleId == result.Value &&
                e.UserId == userId.Value &&
                e.Summary == $"Sync rule created: {srcId} → {tgtId}" &&
                e.EventId != Guid.Empty),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSyncRuleUseCase_WithoutCorrelationId_PublishesEventWithNullCorrelationId()
    {
        var userId = UserId.New();
        var srcId = Guid.NewGuid();
        var tgtId = Guid.NewGuid();
        var command = new CreateSyncRuleCommand(
            userId,
            srcId,
            tgtId,
            false,
            "Busy",
            new FilterPolicy([]));

        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(srcId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.Read);

        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(tgtId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.ReadWrite);

        _correlationIdAccessorMock
            .SetupGet(x => x.CorrelationId)
            .Returns((string?)null);

        await _handler.Handle(command, CancellationToken.None);

        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<SyncRuleCreatedEvent>(e => e.CorrelationId == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSyncRuleUseCase_TargetNotWritable_Throws()
    {
        // Arrange
        var userId = UserId.New();
        var srcId = Guid.NewGuid();
        var tgtId = Guid.NewGuid();
        var command = new CreateSyncRuleCommand(
            userId,
            srcId,
            tgtId,
            false,
            "Busy",
            new FilterPolicy([]));

        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(srcId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.Read);
        
        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(tgtId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.Read); // Not ReadWrite

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(DomainErrorCode.AccessViolation, exception.Code);
        _publishEndpointMock.Verify(x => x.Publish(
            It.IsAny<SyncRuleCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSyncRuleUseCase_WhenPersistFails_DoesNotPublishEvent()
    {
        var userId = UserId.New();
        var srcId = Guid.NewGuid();
        var tgtId = Guid.NewGuid();
        var command = new CreateSyncRuleCommand(
            userId,
            srcId,
            tgtId,
            false,
            "Busy",
            new FilterPolicy([]));

        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(srcId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.Read);

        _connectionServiceMock
            .Setup(x => x.GetCalendarAccessAsync(tgtId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalendarAccess.ReadWrite);

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SyncRule>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB write failed."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _publishEndpointMock.Verify(x => x.Publish(
            It.IsAny<SyncRuleCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
