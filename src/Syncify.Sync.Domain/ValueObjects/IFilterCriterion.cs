using System.Text.Json.Serialization;
using Syncify.Shared.Enums;

namespace Syncify.Sync.Domain.ValueObjects;

public interface IFilterCriterion
{
    [JsonIgnore]
    CalendarAccess MinimumAccess { get; }

    bool Matches(EventSnapshot snapshot);
}
