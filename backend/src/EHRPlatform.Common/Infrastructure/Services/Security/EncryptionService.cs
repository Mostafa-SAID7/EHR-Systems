using System.Security.Cryptography;
using System.Text;
using EHRPlatform.Common.Domain.Constants;
using EHRPlatform.Common.Shared.Utilities.Guards;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// AES-256-GCM encryption and PBKDF2 hashing implementation.
/// Provides authenticated encryption with associated data (AEAD).
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string encryptionKey)
    {
        ArgumentGuard.NotNullOrEmpty(encryptionKey, nameof(encryptionKey));
        if (encryptionKey.Length < 32)
            throw new ArgumentException("Encryption key must be at least 32 characters", nameof(encryptionKey));

        // Derive key from provided key using SHA256
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        try
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = new byte[EncryptionConstants.NonceSizeBytes];
            
            // Generate random nonce
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(nonce);

            // Create cipher
            using var cipher = new AesGcm(_key);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[EncryptionConstants.TagSizeBytes];

            // Encrypt
            cipher.Encrypt(nonce, plaintextBytes, null, ciphertext, tag);

            // Combine nonce + ciphertext + tag and return as Base64
            var result = new byte[EncryptionConstants.NonceSizeBytes + ciphertext.Length + EncryptionConstants.TagSizeBytes];
            Buffer.BlockCopy(nonce, 0, result, 0, EncryptionConstants.NonceSizeBytes);
            Buffer.BlockCopy(ciphertext, 0, result, EncryptionConstants.NonceSizeBytes, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, EncryptionConstants.NonceSizeBytes + ciphertext.Length, EncryptionConstants.TagSizeBytes);

            return ConversionHelper.ToBase64(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        try
        {
            var encryptedData = ConversionHelper.FromBase64Bytes(ciphertext);
            if (encryptedData == null || encryptedData.Length < EncryptionConstants.NonceSizeBytes + EncryptionConstants.TagSizeBytes)
                throw new InvalidOperationException("Invalid ciphertext format");

            // Extract nonce, ciphertext, and tag
            var nonce = new byte[EncryptionConstants.NonceSizeBytes];
            var actualCiphertextLength = encryptedData.Length - EncryptionConstants.NonceSizeBytes - EncryptionConstants.TagSizeBytes;
            var actualCiphertext = new byte[actualCiphertextLength];
            var tag = new byte[EncryptionConstants.TagSizeBytes];

            Buffer.BlockCopy(encryptedData, 0, nonce, 0, EncryptionConstants.NonceSizeBytes);
            Buffer.BlockCopy(encryptedData, EncryptionConstants.NonceSizeBytes, actualCiphertext, 0, actualCiphertextLength);
            Buffer.BlockCopy(encryptedData, EncryptionConstants.NonceSizeBytes + actualCiphertextLength, tag, 0, EncryptionConstants.TagSizeBytes);

            // Decrypt
            using var cipher = new AesGcm(_key);
            var plaintext = new byte[actualCiphertext.Length];
            cipher.Decrypt(nonce, actualCiphertext, null, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Decryption failed - authentication tag verification failed", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Decryption failed", ex);
        }
    }

    public string Hash(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        try
        {
            var salt = new byte[SaltSizeBytes];
            
            // Generate random salt
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            // Derive key using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(KeySizeBytes);

            // Combine salt + hash and return as Base64
            var result = new byte[SaltSizeBytes + hash.Length];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSizeBytes);
            Buffer.BlockCopy(hash, 0, result, SaltSizeBytes, hash.Length);

            return ConversionHelper.ToBase64(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Hashing failed", ex);
        }
    }

    public bool VerifyHash(string value, string hash)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var hashData = ConversionHelper.FromBase64Bytes(hash);
            if (hashData == null || hashData.Length < SaltSizeBytes + KeySizeBytes)
                return false;

            // Extract salt
            var salt = new byte[SaltSizeBytes];
            Buffer.BlockCopy(hashData, 0, salt, 0, SaltSizeBytes);

            // Derive key using same salt
            using var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(KeySizeBytes);

            // Constant-time comparison to prevent timing attacks
            var storedHash = new byte[KeySizeBytes];
            Buffer.BlockCopy(hashData, SaltSizeBytes, storedHash, 0, KeySizeBytes);

            return ConstantTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Constant-time comparison to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}

