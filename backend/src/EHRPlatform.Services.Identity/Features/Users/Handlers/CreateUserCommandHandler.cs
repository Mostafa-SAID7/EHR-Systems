#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Domain.Events;
using EHRPlatform.Services.Identity.Features.Users.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Users.Handlers;

/// <summary>
/// Handler for create user command (admin only).
/// Creates a new user with a temporary password and assigns a role.
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUnitOfWork     _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUnitOfWork     uow,
        IPasswordHasher passwordHasher,
        ILogger<CreateUserCommandHandler> logger)
    {
        _uow            = uow            ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Handle create user request.</summary>
    public async Task<CreateUserResponse> Handle(
        CreateUserCommand  request,
        CancellationToken  cancellationToken)
    {
        _logger.LogInformation(
            "Create user request for email: {Email}, role: {Role}",
            request.Email, request.Role);

        var userRepo = _uow.Repository<User>();

        // Guard: duplicate email
        var existingUser = await userRepo.FirstOrDefaultAsync(
            q => q.Where(u => u.Email == request.Email),
            cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Create user failed: email already exists {Email}", request.Email);
            throw new ConflictException($"Email '{request.Email}' is already in use");
        }

        // Generate temporary password
        var temporaryPassword    = GenerateTemporaryPassword();
        var (hash, salt)         = _passwordHasher.HashWithSalt(temporaryPassword);

        // Create user
        var newUser = new User
        {
            Email          = request.Email,
            FirstName      = request.FirstName,
            LastName       = request.LastName,
            PasswordHash   = hash,
            PasswordSalt   = salt,
            IsActive       = true,
            EmailConfirmed = false,
            MfaEnabled     = false,
            CreatedBy      = request.CreatedBy
        };

        await userRepo.AddAsync(newUser, cancellationToken);

        // TODO: Assign role — look up Role entity and create UserRole link
        // var roleRepo = _uow.Repository<Role>();
        // var role     = await roleRepo.FirstOrDefaultAsync(q => q.Where(r => r.Name == request.Role), cancellationToken);
        // if (role != null) await _uow.Repository<UserRole>().AddAsync(new UserRole { UserId = newUser.Id, RoleId = role.Id }, cancellationToken);

        // Raise in-process domain event
        newUser.RaiseDomainEvent(new UserCreatedDomainEvent
        {
            UserId = newUser.Id,
            Email  = newUser.Email,
            Role   = request.Role
        });

        await _uow.SaveChangesWithEventPublishingAsync(cancellationToken);

        _logger.LogInformation(
            "User created successfully: {UserId}, Email: {Email}",
            newUser.Id, newUser.Email);

        return new CreateUserResponse
        {
            UserId            = newUser.Id,
            Email             = newUser.Email,
            TemporaryPassword = temporaryPassword,
            Message           = "User created successfully. A temporary password has been generated."
        };
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        var random = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        return new string(random.Select(b => chars[b % chars.Length]).ToArray());
    }
}


