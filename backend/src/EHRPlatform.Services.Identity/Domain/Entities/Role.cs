using System;
using System.Collections.Generic;

namespace EHRPlatform.Services.Identity.Domain.Entities
{
    /// <summary>
    /// Service-Specific Role Entity
    /// Represents a role that can be assigned to users (e.g., Doctor, Patient, Admin)
    /// Roles have associated permissions
    /// </summary>
    public class Role
    {
        public Guid Id { get; set; }

        /// <summary>Unique role name (e.g., "Doctor", "Patient", "Admin")</summary>
        public string Name { get; set; }

        /// <summary>Human-readable description of the role</summary>
        public string Description { get; set; }

        /// <summary>Is this role active and available for assignment?</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Users with this role (many-to-many)</summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>Permissions associated with this role (many-to-many)</summary>
        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

        /// <summary>Creation timestamp (UTC)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Service-Specific Permission Entity
    /// Represents an action a user can perform (e.g., "read_patient", "create_appointment")
    /// </summary>
    public class Permission
    {
        public Guid Id { get; set; }

        /// <summary>Unique permission name (e.g., "read_patient")</summary>
        public string Name { get; set; }

        /// <summary>Resource this permission applies to (e.g., "Patient")</summary>
        public string Resource { get; set; }

        /// <summary>Action this permission allows (e.g., "READ", "CREATE", "UPDATE", "DELETE")</summary>
        public string Action { get; set; }

        /// <summary>Is this permission active?</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Roles that have this permission (many-to-many)</summary>
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        /// <summary>Creation timestamp (UTC)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Join table: User-Role relationship (many-to-many)
    /// </summary>
    public class UserRole
    {
        public Guid Id { get; set; }

        /// <summary>Reference to User</summary>
        public Guid UserId { get; set; }

        /// <summary>Reference to Role</summary>
        public Guid RoleId { get; set; }

        /// <summary>When was this role assigned?</summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Who assigned this role? (UserId of the admin)</summary>
        public Guid? AssignedBy { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
    }

    /// <summary>
    /// Join table: Role-Permission relationship (many-to-many)
    /// </summary>
    public class RolePermission
    {
        public Guid Id { get; set; }

        /// <summary>Reference to Role</summary>
        public Guid RoleId { get; set; }

        /// <summary>Reference to Permission</summary>
        public Guid PermissionId { get; set; }

        /// <summary>When was this permission assigned to the role?</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }

    /// <summary>
    /// Refresh Token for JWT token renewal
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }

        /// <summary>Reference to User who owns this token</summary>
        public Guid UserId { get; set; }

        /// <summary>The actual refresh token string</summary>
        public string Token { get; set; }

        /// <summary>When does this refresh token expire?</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>When was this token revoked? (null if still valid)</summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>Was this token replaced by a newer one? If so, store the new token</summary>
        public string ReplacedByToken { get; set; }

        /// <summary>When was this token issued?</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; }

        /// <summary>Is this token still valid (not revoked and not expired)?</summary>
        public bool IsValid()
        {
            return RevokedAt == null && DateTime.UtcNow < ExpiresAt;
        }
    }
}
