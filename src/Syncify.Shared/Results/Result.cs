using Syncify.Shared.Errors;

namespace Syncify.Shared.Results;

public class Result<T> : IResultContract
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ApplicationError? Error { get; }
    object? IResultContract.ValueObject => Value;

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(ApplicationError error) { IsSuccess = false; Error = error; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(ApplicationError error) => new(error);
}