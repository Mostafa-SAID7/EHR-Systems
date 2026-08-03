namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a user identifier
/// </summary>
public sealed class UserId : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the UserId class
    /// </summary>
    /// <param name="value">The GUID value</param>
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Gets the user ID value
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new user ID
    /// </summary>
    /// <returns>A new UserId instance</returns>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Gets the components that make up this value object
    /// </summary>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the string representation of the user ID
    /// </summary>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Implicit conversion from UserId to Guid
    /// </summary>
    public static implicit operator Guid(UserId userId) => userId.Value;

    /// <summary>
    /// Implicit conversion from Guid to UserId
    /// </summary>
    public static implicit operator UserId(Guid value) => new(value);
}
