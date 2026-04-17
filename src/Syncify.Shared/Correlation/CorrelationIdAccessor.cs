using Microsoft.AspNetCore.Http;

namespace Syncify.Shared.Correlation;

public sealed class CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor) : ICorrelationIdAccessor
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public string? CorrelationId => httpContextAccessor.HttpContext?.Request.Headers[CorrelationIdHeader].ToString();
}
