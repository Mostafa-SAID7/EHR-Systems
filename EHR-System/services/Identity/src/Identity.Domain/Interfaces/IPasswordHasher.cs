namespace Identity.Domain.Interfaces;

/// <summary>
/// Service for hashing and verifying passwords
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using a secure algorithm
    /// </summary>
    /// <param name="password">The plain text password</param>
    /// <returns>The password hash</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">The plain text password</param>
    /// <param name="hash">The password hash to verify against</param>
    /// <returns>True if the password matches the hash; otherwise false</returns>
    bool VerifyPassword(string password, string hash);
}
