using System.Text.Json;
using System.Text.Json.Serialization;

namespace Syncify.Sync.Api.Requests;

public sealed record CreateSyncRuleRequest(
    Guid SourceCalendarId,
    Guid TargetCalendarId,
    bool CopyTitle,
    string CustomTitle,
    FilterPolicyRequest FilterPolicy,
    int LookbackDays = 7);

public sealed record UpdateTitleRequest(bool CopyTitle, string CustomTitle);

public sealed record FilterPolicyRequest
{
    public IReadOnlyList<FilterCriterionRequest> Criteria { get; init; } = [];
}

public sealed record FilterCriterionRequest
{
    public string? Type { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement> Properties { get; init; } = new Dictionary<string, JsonElement>();
}
