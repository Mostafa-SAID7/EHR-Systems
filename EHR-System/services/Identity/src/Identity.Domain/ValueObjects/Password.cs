namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a password hash
/// </summary>
public sealed class Password : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the Password class
    /// </summary>
    /// <param name="hash">The password hash value</param>
    /// <exception cref="ArgumentException">Thrown when hash is null or empty</exception>
    public Password(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty", nameof(hash));

        Hash = hash;
    }

    /// <summary>
    /// Gets the password hash
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Gets the components that make up this value object
    /// </summary>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Hash;
    }

    /// <summary>
    /// Returns the string representation of the password hash
    /// </summary>
    public override string ToString() => Hash;
}
