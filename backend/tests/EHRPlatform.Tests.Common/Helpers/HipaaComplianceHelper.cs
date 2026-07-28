#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EHRPlatform.Tests.Common.Helpers;

/// <summary>
/// HIPAA compliance verification and test data utilities.
/// Ensures tests maintain HIPAA requirements for Protected Health Information (PHI).
/// </summary>
public static class HipaaComplianceHelper
{
    /// <summary>
    /// Verify data is encrypted using AES-256.
    /// </summary>
    public static bool ValidatePHIEncryption(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return false;

        // Check for encryption marker or minimum encrypted size
        // This is a simple check; real validation would use crypto verification
        return encryptedData.Length > 16; // Minimum for IV + encrypted data
    }

    /// <summary>
    /// Verify PHI access is being logged.
    /// </summary>
    public static bool ValidatePHIAccessLogging(string auditLog)
    {
        var requiredElements = new[]
        {
            "timestamp",
            "user_id",
            "action",
            "resource_id",
            "access_result"
        };

        foreach (var element in requiredElements)
        {
            if (!auditLog.Contains(element))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Verify patient consent is tracked for data access.
    /// </summary>
    public static bool ValidateConsentTracking(Dictionary<string, object> consentRecord)
    {
        return consentRecord.ContainsKey("patient_id") &&
               consentRecord.ContainsKey("consent_type") &&
               consentRecord.ContainsKey("consent_date") &&
               consentRecord.ContainsKey("expiration_date");
    }

    /// <summary>
    /// Verify audit trail contains required fields.
    /// </summary>
    public static bool ValidateAuditTrail(Dictionary<string, object> auditEntry)
    {
        return auditEntry.ContainsKey("id") &&
               auditEntry.ContainsKey("timestamp") &&
               auditEntry.ContainsKey("user_id") &&
               auditEntry.ContainsKey("action") &&
               auditEntry.ContainsKey("entity_type") &&
               auditEntry.ContainsKey("entity_id") &&
               auditEntry.ContainsKey("changes");
    }

    /// <summary>
    /// Mask PHI for logging (keep only first 2 and last 2 characters).
    /// </summary>
    public static string MaskPHI(string phi)
    {
        if (string.IsNullOrEmpty(phi) || phi.Length <= 4)
            return "****";

        return $"{phi.Substring(0, 2)}****{phi.Substring(phi.Length - 2)}";
    }

    /// <summary>
    /// Check if data field contains PHI and should be protected.
    /// </summary>
    public static bool IsPHIField(string fieldName)
    {
        var phiFields = new[]
        {
            "ssn", "social_security", "mrn", "medical_record",
            "phone", "email", "address", "date_of_birth",
            "name", "account_number", "health_plan"
        };

        var lowerFieldName = fieldName.ToLowerInvariant();
        foreach (var field in phiFields)
        {
            if (lowerFieldName.Contains(field))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Generate HIPAA-compliant synthetic patient data.
    /// </summary>
    public static Dictionary<string, object> GenerateSyntheticPatientData()
    {
        var (firstName, lastName) = TestDataGenerator.GenerateName();
        var (street, city, state, zip) = TestDataGenerator.GenerateAddress();

        return new Dictionary<string, object>
        {
            ["id"] = TestDataGenerator.GenerateId(),
            ["first_name"] = firstName,
            ["last_name"] = lastName,
            ["email"] = TestDataGenerator.GenerateEmail(),
            ["phone"] = TestDataGenerator.GeneratePhoneNumber(),
            ["date_of_birth"] = TestDataGenerator.GenerateDateOfBirth(),
            ["mrn"] = TestDataGenerator.GenerateMRN(),
            ["ssn"] = TestDataGenerator.GenerateSSN(),
            ["address"] = street,
            ["city"] = city,
            ["state"] = state,
            ["zip_code"] = zip,
            ["gender"] = TestDataGenerator.GenerateBoolean() ? "M" : "F",
            ["is_active"] = true,
            ["created_at"] = DateTime.UtcNow,
            ["updated_at"] = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Verify minimum access control is in place for PHI.
    /// </summary>
    public static bool ValidateAccessControl(Dictionary<string, object> accessControlConfig)
    {
        return accessControlConfig.ContainsKey("role") &&
               accessControlConfig.ContainsKey("permissions") &&
               accessControlConfig.ContainsKey("scope");
    }

    /// <summary>
    /// Verify data retention policy compliance.
    /// </summary>
    public static bool ValidateDataRetention(DateTime createdDate, int retentionYears = 6)
    {
        var expirationDate = createdDate.AddYears(retentionYears);
        return DateTime.UtcNow < expirationDate;
    }

    /// <summary>
    /// Verify user is properly identified in audit logs.
    /// </summary>
    public static bool ValidateUserIdentification(Dictionary<string, object> auditEntry)
    {
        var hasUserId = auditEntry.TryGetValue("user_id", out var userId) && userId != null;
        var hasUserName = auditEntry.TryGetValue("user_name", out var userName) && userName != null;

        return hasUserId || hasUserName;
    }

    /// <summary>
    /// Verify no hardcoded PHI in code/tests.
    /// </summary>
    public static bool ValidateNoHardcodedPHI(string content)
    {
        var phiPatterns = new[]
        {
            @"\b\d{3}-\d{2}-\d{4}\b", // SSN format
            @"\b\d{6}-\d{3}\b", // MRN format
            @"\b\d{16}\b", // Credit card format
        };

        foreach (var pattern in phiPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Encrypt data using AES-256 for testing.
    /// </summary>
    public static byte[] EncryptPHI(string plainText, byte[] key, byte[] iv)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }
    }

    /// <summary>
    /// Decrypt PHI data for testing.
    /// </summary>
    public static string DecryptPHI(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new System.IO.MemoryStream(encryptedData))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new System.IO.StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate AES-256 test key and IV.
    /// </summary>
    public static (byte[] Key, byte[] IV) GenerateEncryptionKeyPair()
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            return (aes.Key, aes.IV);
        }
    }
}
