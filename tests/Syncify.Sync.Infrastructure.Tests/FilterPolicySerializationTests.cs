using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Syncify.Sync.Domain.ValueObjects;
using Syncify.Sync.Infrastructure.Serialization;

namespace Syncify.Sync.Infrastructure.Tests;

public class FilterPolicySerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { FilterCriterionJsonConfig.Configure }
        }
    };

    [Fact]
    public void Serialize_PolymorphicCriteria_Works()
    {
        var criteria = new List<IFilterCriterion>
        {
            new KeywordsCriterion(["meeting", "call"]),
            new ExcludesCriterion(["all_day"]),
            new TimeWindowCriterion(9, 17, [DayOfWeek.Monday, DayOfWeek.Tuesday])
        };
        var policy = new FilterPolicy(criteria);

        var json = JsonSerializer.Serialize(policy, JsonOptions);

        Assert.Contains("\"type\":\"keywords\"", json);
        Assert.Contains("\"type\":\"excludes\"", json);
        Assert.Contains("\"type\":\"timeWindow\"", json);
        Assert.Contains("\"keywords\":[\"meeting\",\"call\"]", json);
        Assert.Contains("\"excludes\":[\"all_day\"]", json);
        Assert.Contains("\"startHour\":9", json);
    }

    [Fact]
    public void Deserialize_PolymorphicCriteria_Works()
    {
        var json = """
        {
          "criteria": [
            {
              "type": "keywords",
              "keywords": ["meeting"]
            },
            {
              "type": "timeWindow",
              "startHour": 10,
              "endHour": 15,
              "weekdays": [1, 2, 3]
            }
          ]
        }
        """;

        var policy = JsonSerializer.Deserialize<FilterPolicy>(json, JsonOptions);

        Assert.NotNull(policy);
        Assert.Equal(2, policy.Criteria.Count);

        var keywords = Assert.IsType<KeywordsCriterion>(policy.Criteria[0]);
        Assert.Equal("meeting", keywords.Keywords[0]);

        var timeWindow = Assert.IsType<TimeWindowCriterion>(policy.Criteria[1]);
        Assert.Equal(10, timeWindow.StartHour);
        Assert.Equal(15, timeWindow.EndHour);
        Assert.Contains(DayOfWeek.Monday, timeWindow.Weekdays);
    }
}
