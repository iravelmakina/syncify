namespace Syncify.Shared;

public readonly record struct UserId(Guid Value)
{
    public UserId() : this(Guid.Empty)
        => throw new ArgumentException("UserId cannot be empty.");

    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.");

        return new UserId(value);
    }

    public override string ToString() => Value.ToString();
}