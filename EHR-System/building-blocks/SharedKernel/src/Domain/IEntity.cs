using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Entity interface contract.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Whether entity is soft-deleted.
    /// </summary>
    bool IsDeleted { get; }
}
