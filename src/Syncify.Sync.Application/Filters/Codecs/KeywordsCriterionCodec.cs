using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public sealed class KeywordsCriterionCodec : IFilterCriterionCodec
{
    public string Type => "keywords";
    public Type CriterionType => typeof(KeywordsCriterion);

    public IFilterCriterion Deserialize(IReadOnlyDictionary<string, System.Text.Json.Nodes.JsonNode?> properties)
        => new KeywordsCriterion(FilterCriterionPropertyBag.RequireStringList(properties, "keywords", Type));

    public IReadOnlyDictionary<string, System.Text.Json.Nodes.JsonNode?> Serialize(IFilterCriterion criterion)
    {
        var keywords = (KeywordsCriterion)criterion;
        return new Dictionary<string, System.Text.Json.Nodes.JsonNode?>
        {
            ["keywords"] = FilterCriterionPropertyBag.ToNode(keywords.Keywords)
        };
    }
}