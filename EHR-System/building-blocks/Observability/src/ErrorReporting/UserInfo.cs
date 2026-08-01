namespace EHRPlatform.Observability.ErrorReporting;

/// <summary>
/// User information for error context.
/// Single responsibility: User error context data.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User ID.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// User email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User name.
    /// </summary>
    public string? Name { get; set; }
}
