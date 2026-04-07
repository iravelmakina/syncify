using Syncify.Connections.Domain.Aggregates;
using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Connections.Infrastructure.Persistence.Entities;
using Syncify.Shared;

namespace Syncify.Connections.Infrastructure.Persistence.Mappers;

internal static class CalendarAccountMapper
{
    public static CalendarAccount ToDomain(this CalendarAccountEntity entity)
    {
        var calendars = entity.Calendars
            .Select(c => new CalendarInfo(
                c.Id,
                c.ProviderCalendarId,
                c.Name,
                Enum.Parse<CalendarAccess>(c.Access, ignoreCase: true)))
            .ToList();

        return CalendarAccount.Reconstitute(
            entity.Id,
            new UserId(entity.UserId),
            Enum.Parse<Provider>(entity.Provider, ignoreCase: true),
            new OAuthCredential(entity.RefreshTokenEnc, entity.TokenExpiresAt.UtcDateTime),
            calendars,
            Enum.Parse<ConnectionStatus>(entity.Status, ignoreCase: true),
            entity.CreatedAt.UtcDateTime,
            entity.UpdatedAt.UtcDateTime);
    }

    public static CalendarAccountEntity ToEntity(this CalendarAccount account)
    {
        return new CalendarAccountEntity
        {
            Id = account.Id,
            UserId = account.UserId.Value,
            Provider = account.Provider.ToString(),
            RefreshTokenEnc = account.Credential.RefreshToken,
            TokenExpiresAt = account.Credential.TokenExpiresAt,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            Calendars = account.Calendars
                .Select(c => new CalendarEntity
                {
                    Id = c.Id,
                    AccountId = account.Id,
                    ProviderCalendarId = c.ProviderCalendarId,
                    Name = c.Name,
                    Access = c.Access.ToString()
                })
                .ToList()
        };
    }
}
