#nullable enable

namespace Identity.Contracts.Responses;

/// <summary>
/// User response DTO with full details.
/// </summary>
public class UserResponseDto
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Full name (FirstName + LastName).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// User roles.
    /// </summary>
    public List<RoleDto> Roles { get; set; } = new();

    /// <summary>
    /// Last successful login timestamp.
    /// </summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created the user.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// When the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated the user.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Whether the user's email has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Whether MFA is enabled for the user.
    /// </summary>
    public bool MfaEnabled { get; set; }
}

