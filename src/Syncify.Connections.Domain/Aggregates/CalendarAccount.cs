using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared;

namespace Syncify.Connections.Domain.Aggregates;

public sealed class CalendarAccount
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public Provider Provider { get; private set; }
    public OAuthCredential Credential { get; private set; }
    public ConnectionStatus Status { get; private set; }
    public IReadOnlyList<CalendarInfo> Calendars => _calendars.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private List<CalendarInfo> _calendars = new();

    private CalendarAccount() => Credential = null!; // EF Core

    public static CalendarAccount Create(
        UserId userId,
        Provider provider,
        OAuthCredential credential,
        DateTime utcNow)
    {
        return new CalendarAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            Credential = credential,
            Status = ConnectionStatus.Active,
            _calendars = new List<CalendarInfo>(),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
    
    public void CheckExpiry(DateTime utcNow)
    {
        if (Status == ConnectionStatus.Active && Credential.IsExpired(utcNow))
        {
            Status = ConnectionStatus.Expired;
            UpdatedAt = utcNow;
        }
    }
    
    public void Revoke(DateTime utcNow)
    {
        if (Status == ConnectionStatus.Revoked)
            throw new DomainException("Account is already revoked.");

        Status = ConnectionStatus.Revoked;
        UpdatedAt = utcNow;
    }
    
    public void RefreshCredential(OAuthCredential newCredential, DateTime utcNow)
    {
        if (Status == ConnectionStatus.Revoked)
            throw new DomainException("Cannot refresh credential on a revoked account.");

        Credential = newCredential;
        Status = ConnectionStatus.Active;
        UpdatedAt = utcNow;
    }
    
    public void RefreshCalendars(IReadOnlyList<CalendarInfo> calendars, DateTime utcNow)
    {
        CheckExpiry(utcNow);

        if (Status != ConnectionStatus.Active)
            throw new DomainException("Cannot refresh calendars unless account is active.");

        _calendars = calendars.ToList();
        UpdatedAt = utcNow;
    }

    public static CalendarAccount Reconstitute(
        Guid id,
        UserId userId,
        Provider provider,
        OAuthCredential credential,
        List<CalendarInfo> calendars,
        ConnectionStatus status,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new CalendarAccount
        {
            Id = id,
            UserId = userId,
            Provider = provider,
            Credential = credential,
            _calendars = calendars,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
    }