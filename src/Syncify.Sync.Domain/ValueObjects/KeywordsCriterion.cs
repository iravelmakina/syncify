using System.Text.Json.Serialization;
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;

namespace Syncify.Sync.Domain.ValueObjects;

public sealed record KeywordsCriterion : IFilterCriterion
{
    private const int MaxKeywords = 20;

    public IReadOnlyList<string> Keywords { get; }

    [JsonIgnore]
    public CalendarAccess MinimumAccess => CalendarAccess.Read;

    public KeywordsCriterion(IReadOnlyList<string> keywords)
    {
        if (keywords is not { Count: > 0 })
            throw new DomainException("Keywords list must not be empty.");

        if (keywords.Count > MaxKeywords)
            throw new DomainException($"Keywords list must not exceed {MaxKeywords} items.");

        Keywords = keywords;
    }
}
