namespace Syncify.Shared.Errors;

public abstract record ApplicationError
{
    public record NotFound(string Resource, object Id) : ApplicationError;
    public record Validation(IEnumerable<string> Errors) : ApplicationError;
    public record Conflict(string Message) : ApplicationError;
    public record Forbidden(string Message) : ApplicationError;
}
