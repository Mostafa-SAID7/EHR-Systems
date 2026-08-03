namespace EHRPlatform.Services.Identity.Application.Features.Auth.Commands;

using MediatR;
using EHRPlatform.BuildingBlocks.Security.Jwt;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Persistence;
using EHRPlatform.Services.Identity.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for LoginCommand - Authenticates user with credentials.
/// Uses building-blocks JWT provider for token generation.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IIdentityDbContext context,
        IAuthenticationService authService,
        IJwtTokenProvider jwtTokenProvider,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _authService = authService;
        _jwtTokenProvider = jwtTokenProvider;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for user {Email}", request.Email);

        try
        {
            // Find user by email
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found {Email}", request.Email);
                return new LoginResponse { Success = false, Message = "Invalid email or password" };
            }

            // Check if user is locked
            if (user.Status == "Locked")
            {
                _logger.LogWarning("Login failed: User locked {Email}", request.Email);
                return new LoginResponse { Success = false, Message = "Account is locked. Contact support." };
            }

            // Verify password
            var passwordValid = _authService.VerifyPassword(request.Password, user.PasswordHash);
            if (!passwordValid)
            {
                user.RecordLoginFailure();
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Login failed: Invalid password {Email}", request.Email);
                return new LoginResponse { Success = false, Message = "Invalid email or password" };
            }

            // Record successful login
            user.RecordLoginSuccess();
            await _context.SaveChangesAsync(cancellationToken);

            // Generate tokens using building-blocks JWT provider
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var accessToken = _jwtTokenProvider.GenerateAccessToken(
                user.Id.ToString(), 
                user.FirstName, 
                user.Email, 
                roles);
            var refreshToken = _jwtTokenProvider.GenerateRefreshToken(
                user.Id.ToString(), 
                user.FirstName, 
                user.Email);

            _logger.LogInformation("Login successful for user {Email}", request.Email);

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new LoginResponse { Success = false, Message = "An error occurred during login" };
        }
    }
}
