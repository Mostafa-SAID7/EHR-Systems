namespace Identity.Infrastructure.PasswordPolicy;

/// <summary>
/// Interface for password policy validation.
/// Single responsibility: Password strength requirements contract.
/// </summary>
public interface IPasswordPolicy
{
    /// <summary>
    /// Validate password meets security requirements.
    /// </summary>
    PasswordValidationResult Validate(string password);

    /// <summary>
    /// Get password policy requirements description.
    /// </summary>
    string GetPolicyDescription();
}
