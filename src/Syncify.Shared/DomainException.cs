namespace Syncify.Shared;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}