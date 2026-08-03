namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a token identifier
/// </summary>
public sealed class TokenId : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the TokenId class
    /// </summary>
    /// <param name="value">The GUID value</param>
    public TokenId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Token ID cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Gets the token ID value
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new token ID
    /// </summary>
    /// <returns>A new TokenId instance</returns>
    public static TokenId New() => new(Guid.NewGuid());

    /// <summary>
    /// Gets the components that make up this value object
    /// </summary>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the string representation of the token ID
    /// </summary>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Implicit conversion from TokenId to Guid
    /// </summary>
    public static implicit operator Guid(TokenId tokenId) => tokenId.Value;

    /// <summary>
    /// Implicit conversion from Guid to TokenId
    /// </summary>
    public static implicit operator TokenId(Guid value) => new(value);
}
