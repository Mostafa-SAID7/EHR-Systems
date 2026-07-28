#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EHRPlatform.Tests.Common.Helpers;

/// <summary>
/// Generate realistic test data for HIPAA-compliant testing.
/// All generated data is synthetic and suitable for test environments.
/// </summary>
public static class TestDataGenerator
{
    private static readonly Random Random = new Random();
    private static readonly string[] FirstNames = new[]
    {
        "John", "Jane", "Michael", "Sarah", "Robert", "Emma", "David", "Olivia",
        "James", "Sophia", "Richard", "Isabella", "Joseph", "Mia", "Thomas"
    };

    private static readonly string[] LastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller",
        "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez"
    };

    private static readonly string[] StreetSuffixes = new[] { "St", "Ave", "Blvd", "Rd", "Lane", "Drive", "Court" };
    private static readonly string[] Cities = new[] { "New York", "Los Angeles", "Chicago", "Boston", "Seattle", "Denver" };
    private static readonly string[] States = new[] { "NY", "CA", "IL", "MA", "WA", "CO" };

    /// <summary>
    /// Generate unique GUID.
    /// </summary>
    public static Guid GenerateId()
    {
        return Guid.NewGuid();
    }

    /// <summary>
    /// Generate realistic email address.
    /// </summary>
    public static string GenerateEmail()
    {
        var firstName = FirstNames[Random.Next(FirstNames.Length)].ToLower();
        var lastName = LastNames[Random.Next(LastNames.Length)].ToLower();
        var domain = new[] { "test.com", "example.org", "test.local" }[Random.Next(3)];
        return $"{firstName}.{lastName}{Random.Next(1000)}@{domain}";
    }

    /// <summary>
    /// Generate full name.
    /// </summary>
    public static (string FirstName, string LastName) GenerateName()
    {
        var firstName = FirstNames[Random.Next(FirstNames.Length)];
        var lastName = LastNames[Random.Next(LastNames.Length)];
        return (firstName, lastName);
    }

    /// <summary>
    /// Generate phone number in E.164 format.
    /// </summary>
    public static string GeneratePhoneNumber()
    {
        return $"+1{Random.Next(200, 999)}{Random.Next(200, 999)}{Random.Next(1000, 9999)}";
    }

    /// <summary>
    /// Generate US address.
    /// </summary>
    public static (string Street, string City, string State, string ZipCode) GenerateAddress()
    {
        var street = $"{Random.Next(100, 9999)} {FirstNames[Random.Next(FirstNames.Length)]} {StreetSuffixes[Random.Next(StreetSuffixes.Length)]}";
        var city = Cities[Random.Next(Cities.Length)];
        var state = States[Random.Next(States.Length)];
        var zip = Random.Next(10000, 99999).ToString();
        return (street, city, state, zip);
    }

    /// <summary>
    /// Generate realistic date of birth (ages 18-85).
    /// </summary>
    public static DateTime GenerateDateOfBirth()
    {
        var today = DateTime.Now;
        var start = today.AddYears(-85);
        var range = (today.AddYears(-18) - start).Days;
        return start.AddDays(Random.Next(range));
    }

    /// <summary>
    /// Generate Medical Record Number (MRN).
    /// </summary>
    public static string GenerateMRN()
    {
        // Format: 999999-999 (typical EHR MRN format)
        return $"{Random.Next(100000, 999999)}-{Random.Next(100, 999)}";
    }

    /// <summary>
    /// Generate Social Security Number (synthetic, not real).
    /// </summary>
    public static string GenerateSSN()
    {
        // Format: XXX-XX-XXXX (synthetic data only)
        return $"{Random.Next(100, 999)}-{Random.Next(10, 99)}-{Random.Next(1000, 9999)}";
    }

    /// <summary>
    /// Generate insurance member ID.
    /// </summary>
    public static string GenerateInsuranceId()
    {
        return $"INS{Random.Next(100000000, 999999999)}";
    }

    /// <summary>
    /// Generate random future date for appointments.
    /// </summary>
    public static DateTime GenerateFutureAppointmentDate()
    {
        var start = DateTime.Now.AddDays(1);
        var end = DateTime.Now.AddDays(90);
        var range = (end - start).Days;
        return start.AddDays(Random.Next(range)).AddHours(Random.Next(8, 17));
    }

    /// <summary>
    /// Generate password meeting security requirements.
    /// </summary>
    public static string GeneratePassword()
    {
        // Minimum: 8 chars, upper, lower, digit, special
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
        var password = new StringBuilder();

        password.Append(chars[Random.Next(0, 26)]); // Uppercase
        password.Append(chars[Random.Next(26, 52)]); // Lowercase
        password.Append(chars[Random.Next(52, 62)]); // Digit
        password.Append(chars[Random.Next(62, chars.Length)]); // Special

        for (int i = 0; i < 4; i++)
        {
            password.Append(chars[Random.Next(chars.Length)]);
        }

        return password.ToString();
    }

    /// <summary>
    /// Generate random date in range.
    /// </summary>
    public static DateTime GenerateRandomDate(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Now.AddYears(-1);
        var end = endDate ?? DateTime.Now;
        var range = (end - start).Days;
        return start.AddDays(Random.Next(range)).AddHours(Random.Next(24));
    }

    /// <summary>
    /// Generate diagnosis code (ICD-10 format).
    /// </summary>
    public static string GenerateDiagnosisCode()
    {
        // Simplified ICD-10 format: ABC12.XYZ
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var code = new StringBuilder();

        for (int i = 0; i < 3; i++)
            code.Append(letters[Random.Next(letters.Length)]);

        code.Append(Random.Next(0, 99).ToString().PadLeft(2, '0'));
        code.Append('.');
        code.Append(Random.Next(0, 999).ToString().PadLeft(3, '0'));

        return code.ToString();
    }

    /// <summary>
    /// Generate prescription code.
    /// </summary>
    public static string GeneratePrescriptionCode()
    {
        return $"RX{Random.Next(100000000, 999999999)}";
    }

    /// <summary>
    /// Generate invoice number.
    /// </summary>
    public static string GenerateInvoiceNumber()
    {
        return $"INV{DateTime.Now:yyyyMMdd}{Random.Next(10000, 99999)}";
    }

    /// <summary>
    /// Generate random decimal amount (for billing).
    /// </summary>
    public static decimal GenerateAmount(decimal minAmount = 10, decimal maxAmount = 5000)
    {
        return Convert.ToDecimal(Random.NextDouble() * (double)(maxAmount - minAmount) + (double)minAmount);
    }

    /// <summary>
    /// Generate valid JWT-like token (for testing, not cryptographically secure).
    /// </summary>
    public static string GenerateToken(int expirationMinutes = 60)
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes(@"{""alg"":""HS256"",""typ"":""JWT""}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            @$"{{""sub"":""{GenerateId()}"",""exp"":{(int)(DateTime.UtcNow.AddMinutes(expirationMinutes) - DateTime.UnixEpoch).TotalSeconds}}}"));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("test-secret-key"));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{header}.{payload}")));

        return $"{header}.{payload}.{signature}";
    }

    /// <summary>
    /// Generate list of random strings.
    /// </summary>
    public static List<string> GenerateStringList(int count = 5)
    {
        return Enumerable.Range(0, count)
            .Select(i => $"Item{i}-{GenerateId()}")
            .ToList();
    }

    /// <summary>
    /// Generate random boolean with probability.
    /// </summary>
    public static bool GenerateBoolean(double probabilityOfTrue = 0.5)
    {
        return Random.NextDouble() < probabilityOfTrue;
    }

    /// <summary>
    /// Generate random enum value.
    /// </summary>
    public static T GenerateEnumValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T)) as T[];
        return values![Random.Next(values.Length)];
    }

    /// <summary>
    /// Generate realistic health condition.
    /// </summary>
    public static string GenerateHealthCondition()
    {
        var conditions = new[] { "Hypertension", "Diabetes", "Asthma", "COPD", "Arthritis", "Migraine", "Anxiety", "Depression" };
        return conditions[Random.Next(conditions.Length)];
    }

    /// <summary>
    /// Generate medication name.
    /// </summary>
    public static string GenerateMedicationName()
    {
        var medications = new[] { "Aspirin", "Lisinopril", "Metformin", "Atorvastatin", "Levothyroxine", "Omeprazole", "Sertraline", "Amoxicillin" };
        return medications[Random.Next(medications.Length)];
    }

    /// <summary>
    /// Generate dosage instruction.
    /// </summary>
    public static string GenerateDosage()
    {
        var doses = new[] { "100mg", "250mg", "500mg", "1000mg" };
        var frequencies = new[] { "once daily", "twice daily", "three times daily", "every 6 hours", "as needed" };
        return $"{doses[Random.Next(doses.Length)]} {frequencies[Random.Next(frequencies.Length)]}";
    }

    /// <summary>
    /// Generate patient status.
    /// </summary>
    public static string GeneratePatientStatus()
    {
        var statuses = new[] { "Active", "Inactive", "Deceased", "Transferred" };
        return statuses[Random.Next(statuses.Length)];
    }
}
