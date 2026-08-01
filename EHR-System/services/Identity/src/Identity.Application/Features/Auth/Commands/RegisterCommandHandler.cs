namespace EHRPlatform.Services.Identity.Application.Features.Auth.Commands;

using MediatR;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Persistence;
using EHRPlatform.Services.Identity.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for RegisterCommand - Creates new user account.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityDbContext context,
        IAuthenticationService authService,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for {Email}", request.Email);

        try
        {
            // Check if user exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email already registered"
                };
            }

            // Hash password
            var passwordHash = _authService.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = passwordHash,
                PhoneNumber = request.PhoneNumber,
                Status = "Active",
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            // Assign Patient role by default
            var patientRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Patient", cancellationToken);

            if (patientRole != null)
            {
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = patientRole.Id,
                    AssignedAt = DateTime.UtcNow
                };
                _context.UserRoles.Add(userRole);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Send verification email
            var verificationToken = _authService.GenerateEmailVerificationToken(user.Id);
            await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationToken, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new RegisterResponse
            {
                Success = true,
                UserId = user.Id,
                Message = "Registration successful. Please verify your email.",
                VerificationEmailSent = user.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new RegisterResponse
            {
                Success = false,
                Message = "An error occurred during registration"
            };
        }
    }
}
