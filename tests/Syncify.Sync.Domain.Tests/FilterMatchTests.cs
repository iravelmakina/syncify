using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Domain.Tests;

public class FilterMatchTests
{
    private static readonly DateTime Start = new(2026, 4, 6, 10, 0, 0); // Monday
    private static readonly DateTime End = new(2026, 4, 6, 11, 0, 0);

    private static EventSnapshot Snap(string? title = "Meeting") => new(title, Start, End);
    private static EventSnapshot Snap(DateTime start) => new("Meeting", start, End);

    // --- KeywordsCriterion ---

    [Fact]
    public void Keywords_MatchesTitle_CaseInsensitive()
    {
        var criterion = new KeywordsCriterion(["standup"]);
        Assert.True(criterion.Matches(Snap("Daily Standup")));
    }

    [Fact]
    public void Keywords_NullTitle_ReturnsFalse()
    {
        var criterion = new KeywordsCriterion(["standup"]);
        Assert.False(criterion.Matches(Snap(title: null)));
    }

    [Fact]
    public void Keywords_NoMatch_ReturnsFalse()
    {
        var criterion = new KeywordsCriterion(["standup"]);
        Assert.False(criterion.Matches(Snap("Lunch break")));
    }

    // --- ExcludesCriterion ---

    [Fact]
    public void Excludes_MatchingExclude_ReturnsFalse()
    {
        var criterion = new ExcludesCriterion(["lunch"]);
        Assert.False(criterion.Matches(Snap("Lunch Break")));
    }

    [Fact]
    public void Excludes_NullTitle_ReturnsTrue()
    {
        var criterion = new ExcludesCriterion(["lunch"]);
        Assert.True(criterion.Matches(Snap(title: null)));
    }

    [Fact]
    public void Excludes_NoMatch_ReturnsTrue()
    {
        var criterion = new ExcludesCriterion(["lunch"]);
        Assert.True(criterion.Matches(Snap("Daily Standup")));
    }

    // --- TimeWindowCriterion ---

    [Fact]
    public void TimeWindow_WithinWindow_ReturnsTrue()
    {
        var criterion = new TimeWindowCriterion(9, 17, [DayOfWeek.Monday]);
        Assert.True(criterion.Matches(Snap()));
    }

    [Fact]
    public void TimeWindow_OutsideHours_ReturnsFalse()
    {
        var criterion = new TimeWindowCriterion(9, 17, [DayOfWeek.Monday]);
        Assert.False(criterion.Matches(Snap(new DateTime(2026, 4, 6, 8, 0, 0))));
    }

    [Fact]
    public void TimeWindow_WrongWeekday_ReturnsFalse()
    {
        var criterion = new TimeWindowCriterion(9, 17, [DayOfWeek.Tuesday]);
        Assert.False(criterion.Matches(Snap())); // Monday
    }

    [Fact]
    public void TimeWindow_AtStartBoundary_ReturnsTrue()
    {
        var criterion = new TimeWindowCriterion(10, 17, [DayOfWeek.Monday]);
        Assert.True(criterion.Matches(Snap())); // 10am
    }

    [Fact]
    public void TimeWindow_AtEndBoundary_ReturnsTrue()
    {
        var criterion = new TimeWindowCriterion(9, 11, [DayOfWeek.Monday]);
        Assert.True(criterion.Matches(Snap(new DateTime(2026, 4, 6, 11, 0, 0))));
    }

    // --- FilterPolicy ---

    [Fact]
    public void FilterPolicy_Empty_AlwaysTrue()
    {
        var policy = new FilterPolicy([]);
        Assert.True(policy.Matches(Snap("anything")));
        Assert.True(policy.Matches(Snap(title: null)));
    }

    [Fact]
    public void FilterPolicy_MultipleCriteria_AndLogic()
    {
        var policy = new FilterPolicy([
            new KeywordsCriterion(["standup"]),
            new ExcludesCriterion(["cancelled"]),
            new TimeWindowCriterion(9, 17, [DayOfWeek.Monday])
        ]);

        Assert.True(policy.Matches(Snap("Daily Standup")));
        Assert.False(policy.Matches(Snap("Cancelled Standup")));
        Assert.False(policy.Matches(Snap("Lunch break")));
    }

    [Fact]
    public void FilterPolicy_OneCriterionFails_ReturnsFalse()
    {
        var policy = new FilterPolicy([
            new KeywordsCriterion(["standup"]),
            new TimeWindowCriterion(9, 17, [DayOfWeek.Tuesday]) // wrong day
        ]);

        Assert.False(policy.Matches(Snap("Daily Standup"))); // Monday
    }
}
