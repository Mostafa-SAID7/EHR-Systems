#nullable enable

using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Users.Commands;

/// <summary>
/// Command to create new user (admin only).
/// </summary>
public class CreateUserCommand : ICommand<CreateUserResponse>
{
    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Role to assign to the user.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Department or organization unit.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Admin user ID creating this user.
    /// </summary>
    public Guid CreatedBy { get; set; }
}

