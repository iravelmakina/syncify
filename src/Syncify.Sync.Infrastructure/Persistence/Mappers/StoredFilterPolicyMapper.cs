using System.Text.Json;
using System.Text.Json.Nodes;
using Syncify.Shared.Errors;
using Syncify.Sync.Application.Filters.Codecs;
using Syncify.Sync.Domain.ValueObjects;
using Syncify.Sync.Infrastructure.Persistence.Models;

namespace Syncify.Sync.Infrastructure.Persistence.Mappers;

public static class StoredFilterPolicyMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static FilterPolicy Deserialize(string json)
    {
        try
        {
            var stored = JsonSerializer.Deserialize<StoredFilterPolicy>(json, JsonOptions)
                ?? throw new InvalidOperationException("Stored filter policy payload is invalid.");

            var criteria = stored.Criteria
                .Select(criterion => FilterCriterionCodecRegistry.GetByType(criterion.Type).Deserialize(
                    criterion.Properties.ToDictionary(
                        static x => x.Key,
                        static x => JsonNode.Parse(x.Value.GetRawText()))))
                .ToList();

            return new FilterPolicy(criteria);
        }
        catch (RequestValidationException ex)
        {
            throw new InvalidOperationException("Stored filter policy payload is invalid.", ex);
        }
    }

    public static string Serialize(FilterPolicy policy)
    {
        var stored = new StoredFilterPolicy
        {
            Criteria = policy.Criteria
                .Select(criterion =>
                {
                    var codec = FilterCriterionCodecRegistry.GetByCriterion(criterion);
                    return new StoredFilterCriterion
                    {
                        Type = codec.Type,
                        Properties = codec.Serialize(criterion)
                            .ToDictionary(
                                static x => x.Key,
                                static x => JsonSerializer.SerializeToElement(x.Value, JsonOptions))
                    };
                })
                .ToList()
        };

        return JsonSerializer.Serialize(stored, JsonOptions);
    }
}
