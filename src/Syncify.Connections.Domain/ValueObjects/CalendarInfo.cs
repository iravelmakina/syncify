using Syncify.Connections.Domain.Enums;
using Syncify.Shared;

namespace Syncify.Connections.Domain.ValueObjects;

public sealed record CalendarInfo
{
    public Guid Id { get; }
    public string ProviderCalendarId { get; }
    public string Name { get; }
    public CalendarAccess Access { get; }

    public CalendarInfo(Guid id, string providerCalendarId, string name, CalendarAccess access)
    {
        if (id == Guid.Empty)
            throw new DomainException("Calendar ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(providerCalendarId))
            throw new DomainException("Provider calendar ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Calendar name cannot be empty.");

        if (!Enum.IsDefined(access))
            throw new DomainException("Invalid calendar access level.");

        Id = id;
        ProviderCalendarId = providerCalendarId;
        Name = name;
        Access = access;
    }
}