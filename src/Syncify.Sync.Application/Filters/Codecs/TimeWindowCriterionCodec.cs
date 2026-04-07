using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Filters.Codecs;

public sealed class TimeWindowCriterionCodec : IFilterCriterionCodec
{
    public string Type => "timeWindow";
    public Type CriterionType => typeof(TimeWindowCriterion);

    public IFilterCriterion Deserialize(IReadOnlyDictionary<string, System.Text.Json.Nodes.JsonNode?> properties)
        => new TimeWindowCriterion(
            FilterCriterionPropertyBag.RequireInt(properties, "startHour", Type),
            FilterCriterionPropertyBag.RequireInt(properties, "endHour", Type),
            FilterCriterionPropertyBag.RequireDayOfWeekList(properties, "weekdays", Type));

    public IReadOnlyDictionary<string, System.Text.Json.Nodes.JsonNode?> Serialize(IFilterCriterion criterion)
    {
        var timeWindow = (TimeWindowCriterion)criterion;
        return new Dictionary<string, System.Text.Json.Nodes.JsonNode?>
        {
            ["startHour"] = FilterCriterionPropertyBag.ToNode(timeWindow.StartHour),
            ["endHour"] = FilterCriterionPropertyBag.ToNode(timeWindow.EndHour),
            ["weekdays"] = FilterCriterionPropertyBag.ToNode(timeWindow.Weekdays)
        };
    }
}