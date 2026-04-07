using MediatR;
using Moq;
using Syncify.Connections.Application.Commands.CompleteOAuth;
using Syncify.Connections.Application.Commands.RevokeConnection;
using Syncify.Connections.Application.DTOs;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Application.Queries.ListCalendars;
using Syncify.Connections.Domain.Aggregates;
using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared;

namespace Syncify.Connections.Application.Tests;

public class ConnectionsApplicationTests
{
    private static readonly UserId TestUser = UserId.New();

    private readonly Mock<ICalendarAccountRepository> _repositoryMock = new();
    private readonly Mock<IOAuthProvider> _oAuthProviderMock = new();
    private readonly Mock<ICalendarProvider> _calendarProviderMock = new();

    [Fact]
    public async Task CompleteOAuth_ValidCode_CreatesActiveAccount()
    {
        // Arrange
        var command = new CompleteOAuthCommand(TestUser, "auth-code-123");
        var oAuthResult = new OAuthResult("refresh-token-abc", DateTime.UtcNow.AddHours(1));

        _oAuthProviderMock
            .Setup(x => x.ExchangeCodeAsync("auth-code-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(oAuthResult);

        var handler = new CompleteOAuthCommandHandler(_repositoryMock.Object, _oAuthProviderMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.CreateAsync(
            It.Is<CalendarAccount>(a =>
                a.UserId == TestUser &&
                a.Provider == Provider.Google &&
                a.Status == ConnectionStatus.Active),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeConnection_ExistingAccount_RevokesSuccessfully()
    {
        // Arrange
        var credential = new OAuthCredential("refresh-token", DateTime.UtcNow.AddHours(1));
        var account = CalendarAccount.Create(TestUser, Provider.Google, credential, DateTime.UtcNow);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new RevokeConnectionCommandHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(new RevokeConnectionCommand(account.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ConnectionStatus.Revoked, account.Status);
        _repositoryMock.Verify(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
    }
}