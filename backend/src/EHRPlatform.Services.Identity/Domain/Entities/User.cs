using System;
using System.Collections.Generic;

namespace EHRPlatform.Services.Identity.Domain.Entities
{
    /// <summary>
    /// Service-Specific User Entity
    /// This entity belongs ONLY to the Identity Service.
    /// Other services cannot reference this directly.
    /// Inter-service communication uses UserDto from EHRPlatform.Common.Shared.DTOs
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }

        /// <summary>Unique email address (username)</summary>
        public string Email { get; set; }

        /// <summary>Bcrypt hashed password</summary>
        public string PasswordHash { get; set; }

        /// <summary>User's first name</summary>
        public string FirstName { get; set; }

        /// <summary>User's last name</summary>
        public string LastName { get; set; }

        /// <summary>Formatted phone number</summary>
        public string PhoneNumber { get; set; }

        /// <summary>Has user verified their email address?</summary>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>Is account active (not suspended/deactivated)?</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Last successful login timestamp</summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Associated roles (many-to-many)</summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>Issued refresh tokens</summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>Creation timestamp (UTC)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Last modification timestamp (UTC)</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Soft delete timestamp (UTC) - null if not deleted</summary>
        public DateTime? DeletedAt { get; set; }
    }
}
