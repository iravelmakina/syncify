using Syncify.Shared.Errors;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public static class FilterCriterionCodecRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, IFilterCriterionCodec>> ByType = new(() =>
        typeof(FilterCriterionCodecRegistry).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                typeof(IFilterCriterionCodec).IsAssignableFrom(t))
            .Select(t => (IFilterCriterionCodec)Activator.CreateInstance(t)!)
            .ToDictionary(c => c.Type, StringComparer.OrdinalIgnoreCase));

    private static readonly Lazy<IReadOnlyDictionary<Type, IFilterCriterionCodec>> ByCriterionType = new(() =>
        ByType.Value.Values.ToDictionary(c => c.CriterionType));

    public static IFilterCriterionCodec GetByType(string type)
    {
        if (!ByType.Value.TryGetValue(type, out var codec))
            throw new RequestValidationException($"Unsupported filter criterion type '{type}'.");

        return codec;
    }

    public static IFilterCriterionCodec GetByCriterion(IFilterCriterion criterion)
    {
        if (!ByCriterionType.Value.TryGetValue(criterion.GetType(), out var codec))
            throw new InvalidOperationException(
                $"No filter criterion codec registered for '{criterion.GetType().Name}'.");

        return codec;
    }
}
