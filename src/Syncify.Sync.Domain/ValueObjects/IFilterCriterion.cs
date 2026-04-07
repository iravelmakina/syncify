using System.Text.Json.Serialization;
using Syncify.Shared;

namespace Syncify.Sync.Domain.ValueObjects;

public interface IFilterCriterion
{
    [JsonIgnore]
    CalendarAccess MinimumAccess { get; }
}