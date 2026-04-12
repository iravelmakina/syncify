using Syncify.Sync.Domain.ValueObjects;
using Syncify.Sync.Infrastructure.Persistence.Mappers;

namespace Syncify.Sync.Infrastructure.Tests;

public class FilterPolicySerializationTests
{
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

        var json = StoredFilterPolicyMapper.Serialize(policy);

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

        var policy = StoredFilterPolicyMapper.Deserialize(json);

        Assert.Equal(2, policy.Criteria.Count);

        var keywords = Assert.IsType<KeywordsCriterion>(policy.Criteria[0]);
        Assert.Equal("meeting", keywords.Keywords[0]);

        var timeWindow = Assert.IsType<TimeWindowCriterion>(policy.Criteria[1]);
        Assert.Equal(10, timeWindow.StartHour);
        Assert.Equal(15, timeWindow.EndHour);
        Assert.Contains(DayOfWeek.Monday, timeWindow.Weekdays);
    }
}
