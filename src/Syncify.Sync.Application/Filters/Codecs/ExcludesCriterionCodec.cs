using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public sealed class ExcludesCriterionCodec : IFilterCriterionCodec
{
    public string Type => "excludes";
    public Type CriterionType => typeof(ExcludesCriterion);

    public IFilterCriterion Deserialize(IReadOnlyDictionary<string, System.Text.Json.Nodes.JsonNode?> properties)
        => new ExcludesCriterion(FilterCriterionPropertyBag.RequireStringList(properties, "excludes", Type));

    public IReadOnlyDictionary<string, System.Text.Json.Nodes.JsonNode?> Serialize(IFilterCriterion criterion)
    {
        var excludes = (ExcludesCriterion)criterion;
        return new Dictionary<string, System.Text.Json.Nodes.JsonNode?>
        {
            ["excludes"] = FilterCriterionPropertyBag.ToNode(excludes.Excludes)
        };
    }
}