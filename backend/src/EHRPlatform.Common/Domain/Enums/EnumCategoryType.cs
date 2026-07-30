#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Categorizes enum types across EHR services for centralized reference and mapping.
/// Used by EnumRegistry to organize and discover service enums.
/// </summary>
public enum EnumCategoryType
{
    /// <summary>Entity lifecycle statuses (Draft, Active, Archived, Deleted).</summary>
    Status = 0,

    /// <summary>Resource types or categories (AppointmentType, NoteType, Channel).</summary>
    Type = 1,

    /// <summary>Action or operation types (Create, Update, Delete, Read).</summary>
    Action = 2,

    /// <summary>Communication or notification methods (Email, SMS, Push, InApp).</summary>
    Channel = 3,

    /// <summary>Payment or financial methods (CreditCard, Check, ACH).</summary>
    Method = 4,

    /// <summary>Role or access classifications (Admin, Doctor, Patient).</summary>
    Role = 5,

    /// <summary>Severity or urgency levels (Low, Medium, High, Critical).</summary>
    Level = 6,

    /// <summary>Result or outcome states (Success, Failure, Pending, Warning).</summary>
    Result = 7,

    /// <summary>Classification or grouping categories.</summary>
    Classification = 8,

    /// <summary>Other domain-specific enum types.</summary>
    Other = 9
}

/// <summary>
/// Metadata about an enum type for registry purposes.
/// </summary>
public record EnumMetadata
{
    /// <summary>Fully qualified enum type name.</summary>
    public required string FullTypeName { get; init; }

    /// <summary>Short display name.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Owning service name.</summary>
    public required string ServiceName { get; init; }

    /// <summary>Enum category for grouping.</summary>
    public required EnumCategoryType Category { get; init; }

    /// <summary>Brief description of what this enum represents.</summary>
    public string? Description { get; init; }

    /// <summary>All possible enum values and their slug representations.</summary>
    public Dictionary<string, string> ValueSlugs { get; init; } = new();
}

