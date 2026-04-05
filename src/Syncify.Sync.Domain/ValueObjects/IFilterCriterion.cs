using Syncify.Shared;

namespace Syncify.Sync.Domain.ValueObjects;

public interface IFilterCriterion
{
    CalendarAccess MinimumAccess { get; }
}