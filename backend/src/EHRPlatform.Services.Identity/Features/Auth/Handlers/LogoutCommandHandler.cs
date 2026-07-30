#nullable enable

using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Domain.Exceptions;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Auth.Handlers;

/// <summary>
/// Handler for logout command.
/// Revokes refresh tokens to invalidate sessions.
/// </summary>
public class LogoutCommandHandler : ICommandHandler<LogoutCommand, LogoutResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUnitOfWork uow,
        ILogger<LogoutCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle logout by revoking refresh token.
    /// </summary>
    public async Task<LogoutResponse> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout request for user: {UserId}", request.UserId);

        try
        {
            var rtRepo = _uow.Repository<RefreshToken>();

            // Find and delete the refresh token
            var refreshToken = await rtRepo.FirstOrDefaultAsync(
                q => q.Where(rt => rt.UserId == request.UserId && rt.Token == request.RefreshToken),
                cancellationToken);

            if (refreshToken != null)
            {
                await rtRepo.DeleteAsync(refreshToken, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Refresh token revoked for user: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogWarning("Refresh token not found for user: {UserId}", request.UserId);
            }

            return new LogoutResponse
            {
                Message = "Logged out successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", request.UserId);
            throw;
        }
    }
}

