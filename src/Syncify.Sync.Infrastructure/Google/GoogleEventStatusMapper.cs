namespace Syncify.Sync.Infrastructure.Google;

internal static class GoogleEventStatusMapper
{
    private const string Cancelled = "cancelled";

    public static bool IsCancelled(string? status) => status == Cancelled;
}
