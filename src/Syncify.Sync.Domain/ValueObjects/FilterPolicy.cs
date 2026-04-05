using Syncify.Shared;

namespace Syncify.Sync.Domain.ValueObjects;

public sealed record FilterPolicy
{
    public IReadOnlyList<IFilterCriterion> Criteria { get; }

    public FilterPolicy(IReadOnlyList<IFilterCriterion> criteria)
    {
        Criteria = criteria ?? [];
    }

    public void ValidateAccess(CalendarAccess srcAccess)
    {
        foreach (var criterion in Criteria)
        {
            if (srcAccess < criterion.MinimumAccess)
                throw new DomainException(
                    $"Filter criterion {criterion.GetType().Name} requires at least {criterion.MinimumAccess} access.");
        }
    }

    public T? GetCriterion<T>() where T : class, IFilterCriterion
        => Criteria.OfType<T>().FirstOrDefault();

    public bool HasCriterion<T>() where T : class, IFilterCriterion
        => Criteria.OfType<T>().Any();
}