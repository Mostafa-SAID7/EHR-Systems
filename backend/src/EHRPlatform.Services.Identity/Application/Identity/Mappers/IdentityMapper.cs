#nullable enable

using EHRPlatform.Common.Application.Mapping;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Identity.Mappers;

/// <summary>
/// Mapper for identity service entities to DTOs.
/// Extends MappingServiceBase for consistent mapping patterns.
/// </summary>
public class IdentityMapper : MappingServiceBase<User, UserResponseDto>
{
    /// <summary>
    /// Initialize identity mapper.
    /// </summary>
    public IdentityMapper(ILogger<MappingServiceBase<User, UserResponseDto>> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Map single user entity to response DTO with nested relationships.
    /// </summary>
    public UserResponseDto MapToResponse(User user)
    {
        try
        {
            var dto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
                Roles = MapRoles(user.Roles),
                EmailConfirmed = user.EmailConfirmed,
                MfaEnabled = user.MfaEnabled
            };

            Logger.LogDebug("Mapped user {UserId} to UserResponseDto", user.Id);
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error mapping user {UserId} to UserResponseDto", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Map paginated list of users to response with pagination metadata.
    /// </summary>
    public GetUsersResponse MapToListDto(IList<User> users, int total, int pageNumber, int pageSize)
    {
        try
        {
            var dtos = users.Select(MapToResponse).ToList();

            var response = new GetUsersResponse
            {
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = dtos
            };

            Logger.LogDebug("Mapped {Count} users to paginated list response", users.Count);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error mapping user list to paginated response");
            throw;
        }
    }

    /// <summary>
    /// Map user roles to role DTOs.
    /// </summary>
    private static List<RoleDto> MapRoles(ICollection<UserRole> userRoles)
    {
        return userRoles
            .Select(ur => new RoleDto
            {
                Id = ur.Role?.Id ?? Guid.Empty,
                Name = ur.Role?.Name ?? string.Empty,
                Description = ur.Role?.Description ?? string.Empty
            })
            .ToList();
    }
}

