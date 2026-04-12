using Syncify.Shared;
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;
using Syncify.Sync.Domain.Enums;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Domain.Aggregates;

public sealed class SyncRule
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public Guid SourceCalendarId { get; private set; }
    public Guid TargetCalendarId { get; private set; }
    public bool CopyTitle { get; private set; }
    public string CustomTitle { get; private set; }
    public FilterPolicy FilterPolicy { get; private set; }
    public int LookbackDays { get; private set; }
    public SyncRuleStatus Status { get; private set; }
    public string? SyncCursor { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SyncRule()
    {
        CustomTitle = null!;
        FilterPolicy = null!;
    } // EF Core
    
    public static SyncRule Create(
        UserId userId,
        Guid sourceCalendarId,
        Guid targetCalendarId,
        CalendarAccess srcAccess,
        CalendarAccess tgtAccess,
        bool copyTitle,
        string? customTitle,
        FilterPolicy filterPolicy,
        int lookbackDays,
        DateTime utcNow)
    {
        if (sourceCalendarId == targetCalendarId)
            throw new DomainException("Source and target must reference different calendars.");

        if (string.IsNullOrWhiteSpace(customTitle))
            throw new DomainException("Custom title must not be empty.");

        if (lookbackDays <= 0)
            throw new DomainException("Lookback days must be positive.");

        if (tgtAccess != CalendarAccess.ReadWrite)
            throw new DomainException("Target calendar must have ReadWrite access.", DomainErrorCode.AccessViolation);

        ValidateSourceAccessRestrictions(srcAccess, copyTitle, filterPolicy);

        return new SyncRule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SourceCalendarId = sourceCalendarId,
            TargetCalendarId = targetCalendarId,
            CopyTitle = copyTitle,
            CustomTitle = customTitle ?? "busy",
            FilterPolicy = filterPolicy,
            LookbackDays = lookbackDays,
            Status = SyncRuleStatus.Active,
            SyncCursor = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public static SyncRule Reconstitute(
        Guid id,
        UserId userId,
        Guid sourceCalendarId,
        Guid targetCalendarId,
        bool copyTitle,
        string customTitle,
        FilterPolicy filterPolicy,
        int lookbackDays,
        SyncRuleStatus status,
        string? syncCursor,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new SyncRule
        {
            Id = id,
            UserId = userId,
            SourceCalendarId = sourceCalendarId,
            TargetCalendarId = targetCalendarId,
            CopyTitle = copyTitle,
            CustomTitle = customTitle,
            FilterPolicy = filterPolicy,
            LookbackDays = lookbackDays,
            Status = status,
            SyncCursor = syncCursor,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
    
    public void Archive(DateTime utcNow)
    {
        if (Status != SyncRuleStatus.Active)
            throw new DomainException("Only active rules can be archived.", DomainErrorCode.InvalidState);

        Status = SyncRuleStatus.Archived;
        SyncCursor = null;
        UpdatedAt = utcNow;
    }
    
    public void Resume(CalendarAccess srcAccess, CalendarAccess tgtAccess, DateTime utcNow)
    {
        if (Status != SyncRuleStatus.Archived)
            throw new DomainException("Only archived rules can be resumed.", DomainErrorCode.InvalidState);

        if (tgtAccess != CalendarAccess.ReadWrite)
            throw new DomainException("Target calendar must have ReadWrite access.", DomainErrorCode.AccessViolation);

        ValidateSourceAccessRestrictions(srcAccess, CopyTitle, FilterPolicy);

        Status = SyncRuleStatus.Active;
        UpdatedAt = utcNow;
    }
    
    public void UpdateTitle(bool copyTitle, string customTitle, CalendarAccess srcAccess, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(customTitle))
            throw new DomainException("Custom title must not be empty.");

        ValidateSourceAccessRestrictions(srcAccess, copyTitle, FilterPolicy);

        CopyTitle = copyTitle;
        CustomTitle = customTitle;
        SyncCursor = null;
        UpdatedAt = utcNow;
    }

    public void UpdateSyncCursor(string? cursor, DateTime utcNow)
    {
        SyncCursor = cursor;
        UpdatedAt = utcNow;
    }
    
    private static void ValidateSourceAccessRestrictions(
        CalendarAccess srcAccess, bool copyTitle, FilterPolicy filterPolicy)
    {
        filterPolicy.ValidateAccess(srcAccess);

        if (copyTitle && srcAccess < CalendarAccess.Read)
            throw new DomainException("Cannot copy title when source has less than Read access.", DomainErrorCode.AccessViolation);
    }
}
