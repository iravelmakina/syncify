namespace Syncify.Sync.Infrastructure.Http;

public sealed class ConnectionsServiceOptions
{
    public const string SectionName = "ConnectionsService";

    public string BaseUrl { get; init; } = string.Empty;
}
