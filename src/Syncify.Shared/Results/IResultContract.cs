using Syncify.Shared.Errors;

namespace Syncify.Shared.Results;

public interface IResultContract
{
    bool IsSuccess { get; }
    object? ValueObject { get; }
    ApplicationError? Error { get; }
}
