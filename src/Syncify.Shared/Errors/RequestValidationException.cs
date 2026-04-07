namespace Syncify.Shared.Errors;

public sealed class RequestValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public RequestValidationException(IEnumerable<string> errors)
        : base("The request payload is invalid.")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public RequestValidationException(string error)
        : this([error])
    {
    }
}