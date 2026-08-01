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
/// Handler for MFA verification command.
/// Verifies TOTP code and enables MFA for the user account.
/// </summary>
public class VerifyMfaCommandHandler : ICommandHandler<VerifyMfaCommand, VerifyMfaResponse>
{
    private readonly IUnitOfWork        _uow;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<VerifyMfaCommandHandler> _logger;

    public VerifyMfaCommandHandler(
        IUnitOfWork        uow,
        IEncryptionService encryptionService,
        ILogger<VerifyMfaCommandHandler> logger)
    {
        _uow               = uow               ?? throw new ArgumentNullException(nameof(uow));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Handle MFA verification request.</summary>
    public async Task<VerifyMfaResponse> Handle(
        VerifyMfaCommand  request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("MFA verification attempt for user: {UserId}", request.UserId);

        var userRepo = _uow.Repository<User>();
        var user     = await userRepo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (string.IsNullOrEmpty(user.MfaSecret))
        {
            _logger.LogWarning(
                "MFA verification attempt but MFA not set up for user: {UserId}",
                request.UserId);
            throw new BusinessRuleException("MFA has not been configured for this account");
        }

        // Decrypt TOTP secret and validate code
        var totpSecret = _encryptionService.Decrypt(user.MfaSecret);

        // TODO: Implement proper TOTP (RFC 4226/6238) — currently validates format only
        if (!ValidateTotpCode(request.Code, totpSecret))
        {
            _logger.LogWarning("Invalid TOTP code for user: {UserId}", request.UserId);
            throw new ValidationException("Invalid verification code");
        }

        // Enable MFA
        user.MfaEnabled = true;
        user.UpdatedBy  = request.UserId;

        // Raise in-process domain event
        user.RaiseDomainEvent(new MfaEnabledDomainEvent
        {
            UserId = user.Id
        });

        await _uow.SaveChangesWithEventPublishingAsync(cancellationToken);

        _logger.LogInformation("MFA enabled for user: {UserId}", request.UserId);

        return new VerifyMfaResponse
        {
            Success = true,
            Message = "Multi-factor authentication has been enabled successfully"
        };
    }

    /// <summary>
    /// Validate TOTP code format.
    /// TODO: Replace with real RFC 6238 validation (OtpNet or similar).
    /// </summary>
    private static bool ValidateTotpCode(string code, string secret) =>
        !string.IsNullOrEmpty(code) && code.Length == 6 && int.TryParse(code, out _);
}


