#nullable enable

using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// Extension methods for registering security services.
/// Single responsibility: Manage encryption and password hashing registration.
/// </summary>
public static class SecurityServiceExtensions
{
    /// <summary>
    /// Register encryption and password hashing services.
    /// Encryption key must be 32+ characters (fail-fast validation).
    /// </summary>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        string encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(encryptionKey))
            throw new InvalidOperationException(
                "Encryption key is required. Set EHRCommon:EncryptionKey or ENCRYPTION_KEY env var.");

        services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        return services;
    }
}
