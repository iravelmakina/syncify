using System.Text.Json.Serialization;
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;

namespace Syncify.Sync.Domain.ValueObjects;

public sealed record ExcludesCriterion : IFilterCriterion
{
    public IReadOnlyList<string> Excludes { get; }
    
    [JsonIgnore]
    public CalendarAccess MinimumAccess => CalendarAccess.FreeBusyOnly;

    public ExcludesCriterion(IReadOnlyList<string> excludes)
    {
        if (excludes is not { Count: > 0 })
            throw new DomainException("Excludes list must not be empty.");

        Excludes = excludes;
    }

    public bool Matches(EventSnapshot snapshot)
    {
        if (snapshot.Title is null) return true;
        return !Excludes.Any(e => snapshot.Title.Contains(e, StringComparison.OrdinalIgnoreCase));
    }
}
