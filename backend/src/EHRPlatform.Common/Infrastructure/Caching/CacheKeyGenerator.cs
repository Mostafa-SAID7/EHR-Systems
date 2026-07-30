using System.Security.Cryptography;
using System.Text;

namespace EHRPlatform.Common.Infrastructure.Caching;

/// <summary>
/// Generates consistent cache key patterns for all domain entities.
/// Enables bulk invalidation via pattern matching (e.g., "patient:*").
/// </summary>
public static class CacheKeyGenerator
{
    // Patient cache keys
    public static string PatientKey(Guid id) => $"patient:{id}";
    public static string PatientAllergiesKey(Guid id) => $"patient:{id}:allergies";
    public static string PatientConditionsKey(Guid id) => $"patient:{id}:conditions";
    public static string PatientVitalsKey(Guid id) => $"patient:{id}:vitals";
    public static string PatientDiagnosesKey(Guid id) => $"patient:{id}:diagnoses";
    public static string PatientsListKey => "patients:list";
    public static string PatientsPaginatedKey(int page, int pageSize) => $"patients:paged:{page}:{pageSize}";
    public static string PatientsSearchKey(string searchTerm, int page = 1, int pageSize = 10) =>
        $"patients:search:{Hash(searchTerm)}:{page}:{pageSize}";
    public static string PatientPatternKey => "patient:*";

    // Appointment cache keys
    public static string AppointmentKey(Guid id) => $"appointment:{id}";
    public static string AppointmentsByPatientKey(Guid patientId) => $"appointments:patient:{patientId}";
    public static string AppointmentsByProviderDateKey(Guid providerId, DateTime date) =>
        $"appointments:provider:{providerId}:{date:yyyy-MM-dd}";
    public static string AppointmentPatternKey => "appointment:*";

    // User cache keys
    public static string UserKey(Guid id) => $"user:{id}";
    public static string UserByEmailKey(string email) => $"user:email:{Hash(email)}";
    public static string UserRolesKey(Guid id) => $"user:{id}:roles";
    public static string UserPermissionsKey(Guid id) => $"user:{id}:permissions";
    public static string UserPatternKey => "user:*";

    // Clinical cache keys
    public static string SoapNoteKey(Guid id) => $"soapnote:{id}";
    public static string PatientSoapNotesKey(Guid patientId) => $"patient:{patientId}:soapnotes";
    public static string ClinicalPatternKey => "soapnote:*";

    // Reference data cache keys
    public static string ReferenceDataKey(string dataType) => $"ref:{dataType}";
    public static string CodesKey(string codeType) => $"codes:{codeType}:{Hash(codeType)}";
    public static string ReferencePatternKey => "ref:*";

    // Configuration cache keys
    public static string ConfigurationKey(string name) => $"config:{Hash(name)}";
    public static string ConfigurationPatternKey => "config:*";

    /// <summary>
    /// Hash a string for use in cache keys (to handle special characters safely).
    /// </summary>
    private static string Hash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "null";

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..16]; // First 16 chars of hex
    }
}

