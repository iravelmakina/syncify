namespace Syncify.Shared.Errors;

public enum DomainErrorCode
{
    Validation,
    InvalidState,
    AccessViolation
}

public class DomainException : Exception
{
    public DomainErrorCode Code { get; }

    public DomainException(string message, DomainErrorCode code = DomainErrorCode.Validation)
        : base(message)
    {
        Code = code;
    }
}