namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the Email class
    /// </summary>
    /// <param name="value">The email address value</param>
    /// <exception cref="InvalidEmailException">Thrown when the email format is invalid</exception>
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailException(value ?? "null");

        if (!IsValidEmail(value))
            throw new InvalidEmailException(value);

        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the email address value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Validates if the email format is correct
    /// </summary>
    /// <param name="email">The email to validate</param>
    /// <returns>True if valid; otherwise false</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the components that make up this value object
    /// </summary>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the string representation of the email
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion from Email to string
    /// </summary>
    public static implicit operator string(Email email) => email.Value;
}
