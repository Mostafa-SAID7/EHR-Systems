#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Domain.Events;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Auth.Handlers;

/// <summary>
/// Handler for user registration command.
/// Creates new user, hashes password, and raises a domain event.
/// HIPAA-compliant with audit logging.
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUnitOfWork           _uow;
    private readonly IPasswordHasher       _passwordHasher;
    private readonly IEncryptionService    _encryptionService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork        uow,
        IPasswordHasher    passwordHasher,
        IEncryptionService encryptionService,
        ILogger<RegisterCommandHandler> logger)
    {
        _uow               = uow               ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher    = passwordHasher    ?? throw new ArgumentNullException(nameof(passwordHasher));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle user registration.
    /// </summary>
    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("User registration attempt for email: {Email}", request.Email);

        var userRepo = _uow.Repository<User>();

        // Check if email already exists
        var existingUser = await userRepo.FirstOrDefaultAsync(
            q => q.Where(u => u.Email == request.Email),
            cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: email already registered {Email}", request.Email);
            throw new ConflictException($"Email '{request.Email}' is already registered");
        }

        // Hash password
        var (hash, salt) = _passwordHasher.HashWithSalt(request.Password);

        // Create new user
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
            CreatedBy      = Guid.Empty  // Self-registration
        };

        await userRepo.AddAsync(newUser, cancellationToken);

        // Raise in-process domain event
        newUser.RaiseDomainEvent(new UserRegisteredDomainEvent
        {
            UserId    = newUser.Id,
            Email     = newUser.Email,
            FirstName = newUser.FirstName,
            LastName  = newUser.LastName
        });

        await _uow.SaveChangesWithEventPublishingAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}, Email: {Email}", newUser.Id, newUser.Email);

        return new RegisterResponse
        {
            UserId  = newUser.Id,
            Email   = newUser.Email,
            Message = "Registration successful. Please check your email to confirm your account."
        };
    }
}


