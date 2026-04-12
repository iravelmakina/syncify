using System.Text.Json.Serialization;
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;

namespace Syncify.Sync.Domain.ValueObjects;

public sealed record TimeWindowCriterion : IFilterCriterion
{
    public int StartHour { get; }
    public int EndHour { get; }
    public IReadOnlyList<DayOfWeek> Weekdays { get; }

    [JsonIgnore]
    public CalendarAccess MinimumAccess => CalendarAccess.FreeBusyOnly;

    public TimeWindowCriterion(int startHour, int endHour, IReadOnlyList<DayOfWeek> weekdays)
    {
        if (startHour < 0 || startHour > 23)
            throw new DomainException("Start hour must be between 0 and 23.");

        if (endHour < 0 || endHour > 23)
            throw new DomainException("End hour must be between 0 and 23.");

        if (startHour >= endHour)
            throw new DomainException("Start hour must be less than end hour.");

        if (weekdays is not { Count: > 0 })
            throw new DomainException("Weekdays must not be empty.");

        StartHour = startHour;
        EndHour = endHour;
        Weekdays = weekdays;
    }
}
