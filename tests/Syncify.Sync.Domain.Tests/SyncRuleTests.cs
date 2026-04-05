using Syncify.Shared;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Domain.Tests;

public class SyncRuleTests
{
    private static readonly DateTime Now = new(2026, 4, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly UserId TestUser = UserId.From(Guid.NewGuid());
    private static readonly Guid SourceCalId = Guid.NewGuid();
    private static readonly Guid TargetCalId = Guid.NewGuid();
    private static readonly FilterPolicy EmptyFilter = new([]);

    private static SyncRule CreateValidRule(
        CalendarAccess srcAccess = CalendarAccess.Read,
        CalendarAccess tgtAccess = CalendarAccess.ReadWrite,
        bool copyTitle = false,
        string customTitle = "Busy",
        FilterPolicy? filter = null)
    {
        return SyncRule.Create(
            TestUser, SourceCalId, TargetCalId,
            srcAccess, tgtAccess, copyTitle, customTitle,
            filter ?? EmptyFilter, Now);
    }

    [Fact]
    public void SameSourceAndTarget_Throws()
    {
        var calId = Guid.NewGuid();

        var ex = Assert.Throws<DomainException>(() =>
            SyncRule.Create(TestUser, calId, calId,
                CalendarAccess.Read, CalendarAccess.ReadWrite,
                false, "Busy", EmptyFilter, Now));

        Assert.Contains("different calendars", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FreeBusyOnly_WithKeywords_Throws()
    {
        var filter = new FilterPolicy([new KeywordsCriterion(["meeting"])]);

        var ex = Assert.Throws<DomainException>(() =>
            CreateValidRule(srcAccess: CalendarAccess.FreeBusyOnly, filter: filter));

        Assert.Contains("KeywordsCriterion", ex.Message);
    }

    [Fact]
    public void FreeBusyOnly_WithCopyTitle_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            CreateValidRule(srcAccess: CalendarAccess.FreeBusyOnly, copyTitle: true));

        Assert.Contains("copy title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TimeWindow_StartAfterEnd_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new TimeWindowCriterion(14, 9, [DayOfWeek.Monday]));

        Assert.Contains("Start hour must be less than end hour", ex.Message);
    }

    [Fact]
    public void TargetWithoutReadWrite_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            CreateValidRule(tgtAccess: CalendarAccess.Read));

        Assert.Contains("ReadWrite", ex.Message);
    }

    [Fact]
    public void Archive_ClearsSyncCursor()
    {
        var rule = CreateValidRule();
        rule.UpdateSyncCursor("some-cursor-token", Now);
        Assert.NotNull(rule.SyncCursor);

        rule.Archive(Now);

        Assert.Null(rule.SyncCursor);
    }
}