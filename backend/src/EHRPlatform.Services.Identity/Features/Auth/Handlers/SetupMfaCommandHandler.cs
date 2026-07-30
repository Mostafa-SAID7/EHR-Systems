#nullable enable

using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Domain.Exceptions;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Auth.Handlers;

/// <summary>
/// Handler for MFA setup command.
/// Generates TOTP secret and backup codes for multi-factor authentication.
/// </summary>
public class SetupMfaCommandHandler : ICommandHandler<SetupMfaCommand, SetupMfaResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SetupMfaCommandHandler> _logger;

    public SetupMfaCommandHandler(
        IUnitOfWork uow,
        IEncryptionService encryptionService,
        ILogger<SetupMfaCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle MFA setup request.
    /// </summary>
    public async Task<SetupMfaResponse> Handle(
        SetupMfaCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("MFA setup request for user: {UserId}", request.UserId);

        var userRepo = _uow.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        // Generate TOTP secret
        var totpSecret = GenerateTotpSecret();
        
        // Encrypt and store secret
        var encryptedSecret = _encryptionService.Encrypt(totpSecret);
        user.MfaSecret = encryptedSecret;

        // Generate backup codes
        var backupCodes = GenerateBackupCodes(10);
        var encryptedBackupCodes = _encryptionService.Encrypt(string.Join(",", backupCodes));
        user.MfaSecretBackupCodes = encryptedBackupCodes;

        // MFA not yet enabled - user must verify with code first
        user.MfaEnabled = false;
        user.UpdatedBy = request.UserId;

        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("MFA setup initiated for user: {UserId}", request.UserId);

        return new SetupMfaResponse
        {
            Secret = totpSecret,
            QrCodeUrl = GenerateQrCodeUri(user.Email, totpSecret),
            BackupCodes = backupCodes
        };
    }

    /// <summary>
    /// Generate random TOTP secret (base32 encoded).
    /// </summary>
    private static string GenerateTotpSecret()
    {
        const int secretLength = 32;
        var secret = new byte[secretLength];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(secret);
        }

        // TODO: Base32 encode the secret for TOTP compatibility
        return Convert.ToBase64String(secret);
    }

    /// <summary>
    /// Generate backup recovery codes.
    /// </summary>
    private static List<string> GenerateBackupCodes(int count)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var code = Convert.ToHexString(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(4));
            codes.Add($"{code[..4]}-{code[4..]}");
        }
        return codes;
    }

    /// <summary>
    /// Generate QR code URI for TOTP app provisioning.
    /// </summary>
    private static string GenerateQrCodeUri(string userEmail, string secret)
    {
        // TODO: Generate proper otpauth:// URI for QR code
        return $"otpauth://totp/EHR%20Platform:{userEmail}?secret={secret}&issuer=EHR%20Platform";
    }
}

