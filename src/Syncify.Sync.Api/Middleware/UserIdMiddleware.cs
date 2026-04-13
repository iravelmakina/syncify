using Syncify.Shared;

namespace Syncify.Sync.Api.Middleware;

public sealed class UserIdMiddleware(RequestDelegate next)
{
    public const string UserIdKey = "UserId";

    private static readonly HashSet<string> SkipPaths = ["/health", "/openapi", "/scalar"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (SkipPaths.Any(p => path.Equals(p) || path.StartsWithSegments(p)))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-User-ID", out var header)
            || !Guid.TryParse(header, out var parsed)
            || parsed == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid X-User-ID header." });
            return;
        }

        context.Items[UserIdKey] = UserId.From(parsed);
        await next(context);
    }
}

public static class UserIdMiddlewareExtensions
{
    public static UserId GetUserId(this HttpContext context)
        => (UserId)context.Items[UserIdMiddleware.UserIdKey]!;
}