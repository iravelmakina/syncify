using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared;

namespace Syncify.Connections.Domain.Aggregates;

public sealed class CalendarAccount
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public Provider Provider { get; private set; }
    public string ProviderAccountId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
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
        string providerAccountId,
        string email,
        OAuthCredential credential,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(providerAccountId))
            throw new DomainException("Provider account ID is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        return new CalendarAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderAccountId = providerAccountId,
            Email = email,
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
            throw new DomainException("Account is already revoked.", DomainErrorCode.InvalidState);

        Status = ConnectionStatus.Revoked;
        UpdatedAt = utcNow;
    }
    
    public void Reconnect(OAuthCredential newCredential, DateTime utcNow)
    {
        Credential = newCredential;
        Status = ConnectionStatus.Active;
        UpdatedAt = utcNow;
    }
    
    public void RefreshCalendars(IReadOnlyList<CalendarInfo> calendars, DateTime utcNow)
    {
        CheckExpiry(utcNow);

        if (Status != ConnectionStatus.Active)
            throw new DomainException("Cannot refresh calendars unless account is active.", DomainErrorCode.InvalidState);

        _calendars = calendars.ToList();
        UpdatedAt = utcNow;
    }

    public static CalendarAccount Reconstitute(
        Guid id,
        UserId userId,
        Provider provider,
        string providerAccountId,
        string email,
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
            ProviderAccountId = providerAccountId,
            Email = email,
            Credential = credential,
            _calendars = calendars,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
    }
