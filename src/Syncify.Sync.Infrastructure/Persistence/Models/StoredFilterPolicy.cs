using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Syncify.Sync.Infrastructure.Persistence.Models;

internal sealed record StoredFilterPolicy
{
    public IReadOnlyList<StoredFilterCriterion> Criteria { get; init; } = [];
}

internal sealed record StoredFilterCriterion
{
    public string Type { get; init; } = string.Empty;

    [JsonExtensionData]
    public IDictionary<string, JsonNode?> Properties { get; init; } = new Dictionary<string, JsonNode?>();
}