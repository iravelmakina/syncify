using Syncify.Connections.Domain.Aggregates;
using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared;
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;

namespace Syncify.Connections.Domain.Tests;

public class CalendarAccountTests
{
    private static readonly DateTime Now = new(2026, 4, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly UserId TestUser = UserId.From(Guid.NewGuid());

    private static CalendarAccount CreateActiveAccount(DateTime tokenExpiresAt)
    {
        var credential = new OAuthCredential("refresh-token-abc", tokenExpiresAt);
        return CalendarAccount.Create(
            TestUser,
            Provider.Google,
            "google-account-123",
            "user@example.com",
            credential,
            Now);
    }
    
    [Fact]
    public void ActiveAccount_WithExpiredToken_TransitionsToExpired()
    {
        var account = CreateActiveAccount(tokenExpiresAt: Now.AddHours(-1));
        var futureNow = Now.AddMinutes(1);

        account.CheckExpiry(futureNow);

        Assert.Equal(ConnectionStatus.Expired, account.Status);
    }
    
    [Fact]
    public void RevokedAccount_Reconnect_TransitionsToActive()
    {
        var account = CreateActiveAccount(tokenExpiresAt: Now.AddHours(1));
        account.Revoke(Now);

        var newCredential = new OAuthCredential("new-refresh-token", Now.AddHours(2));

        account.Reconnect(newCredential, Now);

        Assert.Equal(ConnectionStatus.Active, account.Status);
        Assert.Equal(newCredential, account.Credential);
    }
    
    [Fact]
    public void ExpiredAccount_Reconnect_TransitionsToActive()
    {
        var account = CreateActiveAccount(tokenExpiresAt: Now.AddHours(-1));
        account.CheckExpiry(Now);
        Assert.Equal(ConnectionStatus.Expired, account.Status);

        var newCredential = new OAuthCredential("new-refresh-token", Now.AddHours(2));
        account.Reconnect(newCredential, Now);

        Assert.Equal(ConnectionStatus.Active, account.Status);
    }
    
    [Fact]
    public void ExpiredAccount_RefreshCalendars_Throws()
    {
        var account = CreateActiveAccount(tokenExpiresAt: Now.AddHours(-1));
        account.CheckExpiry(Now);
        Assert.Equal(ConnectionStatus.Expired, account.Status);

        var calendars = new List<CalendarInfo>
        {
            new(Guid.NewGuid(), "cal-1", "Work", CalendarAccess.ReadWrite)
        };

        var ex = Assert.Throws<DomainException>(() =>
            account.RefreshCalendars(calendars, Now));

        Assert.Equal(DomainErrorCode.InvalidState, ex.Code);
    }
}
