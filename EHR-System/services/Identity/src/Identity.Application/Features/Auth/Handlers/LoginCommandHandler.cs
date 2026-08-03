#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.BuildingBlocks.Security.Jwt;
using EHRPlatform.Services.Identity.Contracts.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Handlers;

/// <summary>
/// Validates credentials, issues JWT + refresh token, and handles MFA.
/// HIPAA-compliant with audit logging.
/// Uses building-blocks JWT provider for token generation.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork      _uow;
    private readonly IPasswordHasher  _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork      uow,
        IPasswordHasher  passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        ILogger<LoginCommandHandler> logger)
    {
        _uow             = uow             ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher  = passwordHasher  ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenProvider = jwtTokenProvider ?? throw new ArgumentNullException(nameof(jwtTokenProvider));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var userRepo = _uow.Repository<User>();
        var user = await userRepo.FirstOrDefaultAsync(
            q => q.Where(u => u.Email == request.Email && u.IsActive),
            cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password");

        if (user.IsLocked())
        {
            _logger.LogWarning("Locked account login attempt: {Email}", request.Email);
            throw new UnauthorizedException("Account is temporarily locked due to multiple failed attempts");
        }

        // Verify password using the separate-salt path
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.Lock();
                _logger.LogWarning("Account locked after 5 failed attempts: {Email}", request.Email);
            }
            await _uow.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid email or password");
        }

        // MFA gate – don't issue tokens until second factor is passed
        if (user.MfaEnabled)
        {
            _logger.LogInformation("MFA required for user: {UserId}", user.Id);
            return new LoginResponse { MfaRequired = true, AccessToken = string.Empty, RefreshToken = string.Empty, ExpiresIn = 0 };
        }

        // Generate tokens using building-blocks JWT provider
        var roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "User").ToList() ?? new List<string> { "User" };
        var accessToken  = _jwtTokenProvider.GenerateAccessToken(user.Id.ToString(), user.FirstName, user.Email, roles);
        var refreshToken = GenerateRefreshToken();

        // Store hashed refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId    = user.Id,
            Token     = _passwordHasher.Hash(refreshToken, string.Empty),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedBy = user.Id
        };
        await _uow.Repository<RefreshToken>().AddAsync(refreshTokenEntity, cancellationToken);

        // Update last-login metadata
        user.LastLogin           = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.UpdatedBy           = user.Id;
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Login successful for user: {UserId}", user.Id);

        return new LoginResponse
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn    = 3600, // 1 hour (hardcoded; can be pulled from JwtSettings if needed)
            MfaRequired  = false,
            User         = new UserResponseDto
            {
                Id             = user.Id,
                Email          = user.Email,
                FirstName      = user.FirstName,
                LastName       = user.LastName,
                IsActive       = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                MfaEnabled     = user.MfaEnabled,
                LastLogin      = user.LastLogin,
                CreatedAt      = user.CreatedAt
            }
        };
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
}
