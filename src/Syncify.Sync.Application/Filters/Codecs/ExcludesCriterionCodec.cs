using System.Text.Json.Nodes;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public sealed class ExcludesCriterionCodec : IFilterCriterionCodec
{
    public string Type => "excludes";
    public Type CriterionType => typeof(ExcludesCriterion);

    public IFilterCriterion Deserialize(IReadOnlyDictionary<string, JsonNode?> properties)
        => new ExcludesCriterion(FilterCriterionPropertyBag.RequireStringList(properties, "excludes", Type));

    public IReadOnlyDictionary<string, JsonNode?> Serialize(IFilterCriterion criterion)
    {
        var excludes = (ExcludesCriterion)criterion;
        return new Dictionary<string, JsonNode?>
        {
            ["excludes"] = FilterCriterionPropertyBag.ToNode(excludes.Excludes)
        };
    }
}