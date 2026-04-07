using System.Text.Json;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Domain.ValueObjects;

public static class FilterCriterionRegistry
{
    public static IEnumerable<Type> GetDerivedTypes()
    {
        var baseType = typeof(IFilterCriterion);

        return baseType.Assembly.GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                t.IsAssignableTo(baseType));
    }

    public static string GetDiscriminator(Type type)
    {
        return JsonNamingPolicy.CamelCase
            .ConvertName(type.Name.Replace("Criterion", ""));
    }
}
