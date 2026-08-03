#nullable enable

namespace Identity.Contracts.Responses;

/// <summary>
/// Update user response DTO.
/// </summary>
public class UpdateUserResponse
{
    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = "User updated successfully";

    /// <summary>
    /// Timestamp when the user was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

