using System.Text.Json.Nodes;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public interface IFilterCriterionCodec
{
    string Type { get; }
    Type CriterionType { get; }
    IFilterCriterion Deserialize(IReadOnlyDictionary<string, JsonNode?> properties);
    IReadOnlyDictionary<string, JsonNode?> Serialize(IFilterCriterion criterion);
}