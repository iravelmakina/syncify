using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Infrastructure.Serialization;

public static class FilterCriterionJsonConfig
{
    public static void Configure(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(IFilterCriterion)) return;

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "type",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (var type in FilterCriterionRegistry.GetDerivedTypes())
        {
            var discriminator = FilterCriterionRegistry.GetDiscriminator(type);
            typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(type, discriminator));
        }
    }
}
