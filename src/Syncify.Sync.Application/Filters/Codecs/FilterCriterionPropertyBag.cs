using System.Text.Json;
using System.Text.Json.Nodes;
using Syncify.Shared.Errors;

namespace Syncify.Sync.Application.Filters.Codecs;

internal static class FilterCriterionPropertyBag
{
    public static IReadOnlyList<string> RequireStringList(
        IReadOnlyDictionary<string, JsonNode?> properties,
        string fieldName,
        string criterionType)
        => Require<List<string>>(properties, fieldName, criterionType);

    public static IReadOnlyList<DayOfWeek> RequireDayOfWeekList(
        IReadOnlyDictionary<string, JsonNode?> properties,
        string fieldName,
        string criterionType)
        => Require<List<DayOfWeek>>(properties, fieldName, criterionType);

    public static int RequireInt(
        IReadOnlyDictionary<string, JsonNode?> properties,
        string fieldName,
        string criterionType)
        => Require<int>(properties, fieldName, criterionType);

    public static JsonNode ToNode<T>(T value)
        => JsonSerializer.SerializeToNode(value)
            ?? throw new InvalidOperationException("Failed to serialize filter criterion property.");

    private static T Require<T>(
        IReadOnlyDictionary<string, JsonNode?> properties,
        string fieldName,
        string criterionType)
    {
        if (!properties.TryGetValue(fieldName, out var node) || node is null)
            throw new RequestValidationException(
                $"Filter criterion '{criterionType}' must include '{fieldName}'.");

        try
        {
            var value = node.Deserialize<T>();
            if (value is null)
                throw new RequestValidationException(
                    $"Filter criterion '{criterionType}' has an invalid '{fieldName}' value.");

            return value;
        }
        catch (JsonException)
        {
            throw new RequestValidationException(
                $"Filter criterion '{criterionType}' has an invalid '{fieldName}' value.");
        }
    }
}
