namespace Syncify.Shared.Correlation;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}
