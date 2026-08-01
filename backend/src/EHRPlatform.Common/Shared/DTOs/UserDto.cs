using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Inter-Service User Communication
    /// Used ONLY for authentication/authorization between services.
    /// NOT mapped to Identity Service's internal User entity.
    /// Services receive this via:
    /// 1. JWT claims (embedded in token)
    /// 2. Kafka events (UserCreated, UserRoleAssigned, etc.)
    /// 3. REST API calls from Identity Service
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public string[] Roles { get; set; }
        public string[] Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Event: User Created
    /// Published by Identity Service when a new user is registered
    /// Subscribed by: Notification Service (to create user preferences)
    /// </summary>
    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public UserDto UserData { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: User Updated
    /// Published by Identity Service when user details change
    /// Subscribed by: All services that cache user information
    /// </summary>
    public class UserUpdatedEvent
    {
        public Guid UserId { get; set; }
        public UserDto UserData { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: User Role Assigned
    /// Published by Identity Service when a role is assigned
    /// Subscribed by: Services that need to invalidate user permission cache
    /// </summary>
    public class UserRoleAssignedEvent
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: User Deactivated
    /// Published by Identity Service when a user is deactivated
    /// Subscribed by: All services (to revoke access)
    /// </summary>
    public class UserDeactivatedEvent
    {
        public Guid UserId { get; set; }
        public string Reason { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
