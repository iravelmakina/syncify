using Syncify.Api.Requests;

using Syncify.Shared.Enums;
using Syncify.Shared.Errors;
using Syncify.Shared.Ports;
using Syncify.Sync.Application.Filters.Codecs;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Api.Mappers;

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
        var properties = request.Properties.ToDictionary(x => x.Key, x => x.Value);

        return FilterCriterionCodecRegistry.GetByType(type).Deserialize(properties);
    }
}
