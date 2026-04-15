using Syncify.Sync.Api.Requests;
using Syncify.Shared.Errors;
using Syncify.Sync.Application.Filters.Codecs;
using Syncify.Sync.Domain.ValueObjects;
using System.Text.Json.Nodes;

namespace Syncify.Sync.Api.Mappers;

internal static class FilterPolicyRequestMapper
{
    public static FilterPolicy ToDomain(this FilterPolicyRequest request)
    {
        var criteria = request.Criteria
            .Select((criterion, index) => ToDomain(criterion, index))
            .ToList();

        return new FilterPolicy(criteria);
    }

    private static IFilterCriterion ToDomain(FilterCriterionRequest request, int index)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
            throw new RequestValidationException(
                $"Filter criterion at index {index} must include a valid 'type'.");

        var type = request.Type.Trim();
        var properties = request.Properties.ToDictionary(
            static x => x.Key,
            static x => JsonNode.Parse(x.Value.GetRawText()));

        return FilterCriterionCodecRegistry.GetByType(type).Deserialize(properties);
    }
}
