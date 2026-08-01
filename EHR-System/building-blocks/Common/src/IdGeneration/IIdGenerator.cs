namespace EHRPlatform.Common.IdGeneration;

/// <summary>
/// Interface for ID generation.
/// Single responsibility: ID generation contract.
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// Generate a new unique identifier.
    /// </summary>
    string GenerateId();

    /// <summary>
    /// Generate a new GUID-based identifier.
    /// </summary>
    string GenerateGuid();

    /// <summary>
    /// Generate a new ulid (sortable UUID).
    /// </summary>
    string GenerateUlid();

    /// <summary>
    /// Generate sequential ID for database operations.
    /// </summary>
    long GenerateSequentialId();

    /// <summary>
    /// Generate short alphanumeric identifier.
    /// </summary>
    string GenerateShortId();
}
