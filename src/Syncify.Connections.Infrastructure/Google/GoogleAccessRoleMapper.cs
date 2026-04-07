using Syncify.Shared.Enums;

namespace Syncify.Connections.Infrastructure.Google;

internal static class GoogleAccessRoleMapper
{
    private const string FreeBusyReader = "freeBusyReader";
    private const string Reader = "reader";
    private const string Writer = "writer";
    private const string Owner = "owner";

    public static CalendarAccess ToDomain(string accessRole)
    {
        return accessRole switch
        {
            FreeBusyReader => CalendarAccess.FreeBusyOnly,
            Reader => CalendarAccess.Read,
            Writer or Owner => CalendarAccess.ReadWrite,
            _ => CalendarAccess.FreeBusyOnly
        };
    }
}
