namespace Syncify.Sync.Infrastructure.Polling;

public sealed class SyncPollerOptions
{
    public const string SectionName = "SyncPoller";

    public int IntervalSeconds { get; init; } = 300;
}
