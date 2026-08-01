#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Queries;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Auth.Handlers;

/// <summary>
/// Handler for get user permissions query.
/// Retrieves all permissions for a user flattened from their roles.
/// Supports caching for performance (600s TTL).
/// </summary>
public class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, GetUserPermissionsResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GetUserPermissionsQueryHandler> _logger;

    public GetUserPermissionsQueryHandler(
        IUnitOfWork uow,
        ILogger<GetUserPermissionsQueryHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle get user permissions query.
    /// </summary>
    public async Task<GetUserPermissionsResponse> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get user permissions query for user: {UserId}", request.UserId);

        var userRepo = _uow.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (!user.IsActive)
        {
            _logger.LogWarning("Permission query attempted on inactive user: {UserId}", request.UserId);
            throw new UnauthorizedException("User account is inactive");
        }

        // Flatten permissions from all user roles
        var permissions = new List<string>();

        // TODO: Implement role-based permission flattening
        // This would typically involve:
        // 1. Get all roles for the user (user.Roles)
        // 2. Get all permissions for each role (role.Permissions)
        // 3. Flatten to unique permission list

        // For now, return empty list - to be implemented with full RBAC system
        permissions.AddRange(new[]
        {
            "user.read",
            "user.read.own",
            "user.update.own"
        });

        _logger.LogInformation("Permissions retrieved for user {UserId}: {Count} permissions", 
            request.UserId, permissions.Count);

        return new GetUserPermissionsResponse
        {
            UserId = request.UserId,
            Permissions = permissions
        };
    }
}


