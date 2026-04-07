using Syncify.Shared.Enums;
using Syncify.Shared.Errors;

namespace Syncify.Sync.Domain.ValueObjects;

public sealed record FilterPolicy(IReadOnlyList<IFilterCriterion> Criteria)
{
    public void ValidateAccess(CalendarAccess srcAccess)
    {
        foreach (var criterion in Criteria)
        {
            if (srcAccess < criterion.MinimumAccess)
                throw new DomainException(
                    $"Filter criterion {criterion.GetType().Name} requires at least {criterion.MinimumAccess} access.",
                    DomainErrorCode.AccessViolation);
        }
    }

    public T? GetCriterion<T>() where T : class, IFilterCriterion
        => Criteria.OfType<T>().FirstOrDefault();

    public bool HasCriterion<T>() where T : class, IFilterCriterion
        => Criteria.OfType<T>().Any();
}
