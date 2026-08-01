#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Contracts.Responses;

namespace EHRPlatform.Services.Identity.Application.Features.Users.Commands;

/// <summary>
/// Command to update user profile information.
/// </summary>
public class UpdateUserCommand : ICommand<UpdateUserResponse>
{
    /// <summary>
    /// User ID to update.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Updated first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Updated last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Updated email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Admin user ID performing this update.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}




