using System.Text.Json.Nodes;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public sealed class KeywordsCriterionCodec : IFilterCriterionCodec
{
    public string Type => "keywords";
    public Type CriterionType => typeof(KeywordsCriterion);

    public IFilterCriterion Deserialize(IReadOnlyDictionary<string, JsonNode?> properties)
        => new KeywordsCriterion(FilterCriterionPropertyBag.RequireStringList(properties, "keywords", Type));

    public IReadOnlyDictionary<string, JsonNode?> Serialize(IFilterCriterion criterion)
    {
        var keywords = (KeywordsCriterion)criterion;
        return new Dictionary<string, JsonNode?>
        {
            ["keywords"] = FilterCriterionPropertyBag.ToNode(keywords.Keywords)
        };
    }
}
