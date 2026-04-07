using Moq;
using Syncify.Shared;
using Syncify.Sync.Application.Commands.CreateSyncRule;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Tests;

public class CreateSyncRuleTests
{
    private readonly Mock<ISyncRuleRepository> _repositoryMock = new();
    private readonly Mock<IConnectionService> _connectionServiceMock = new();
    private readonly CreateSyncRuleCommandHandler _handler;

    public CreateSyncRuleTests()
    {
        _handler = new CreateSyncRuleCommandHandler(
            _repositoryMock.Object,
            _connectionServiceMock.Object);
    }

    [Fact]
    public async Task CreateSyncRuleUseCase_ValidAccess_CreatesActiveRule()
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
            .ReturnsAsync(CalendarAccess.ReadWrite);

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
    }
}
