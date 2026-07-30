#nullable enable

using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Domain.Exceptions;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Users.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Users.Handlers;

/// <summary>
/// Handler for update user command.
/// Updates user profile information with audit trail.
/// </summary>
public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUnitOfWork uow,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle update user request.
    /// </summary>
    public async Task<UpdateUserResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update user request for user: {UserId}", request.UserId);

        var userRepo = _uow.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        // If email is changing, check for uniqueness
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var existingUser = await userRepo.FirstOrDefaultAsync(
                q => q.Where(u => u.Email == request.Email),
                cancellationToken);

            if (existingUser != null)
            {
                _logger.LogWarning("Update user failed: email already in use {Email}", request.Email);
                throw new ConflictException($"Email '{request.Email}' is already in use");
            }

            user.Email = request.Email;
        }

        // Update allowed fields
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        user.IsActive = request.IsActive;
        user.UpdatedBy = request.UpdatedBy;

        // Publish domain event
        user.RaiseDomainEvent(new UserUpdatedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        });

        await _uow.SaveChangesWithEventPublishingAsync(cancellationToken);

        _logger.LogInformation("User updated successfully: {UserId}", request.UserId);

        return new UpdateUserResponse
        {
            Message = "User updated successfully",
            UpdatedAt = user.UpdatedAt
        };
    }
}

/// <summary>
/// Domain event published when user is updated.
/// </summary>
public class UserUpdatedEvent : EHRPlatform.Common.Entities.DomainEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

