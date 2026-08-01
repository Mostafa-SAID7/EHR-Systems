using Microsoft.AspNetCore.Authorization;

namespace EHRPlatform.Security.Authorization;

/// <summary>
/// Authorization policy definitions for EHR platform.
/// Defines what operations require which permissions/roles.
/// </summary>
public static class AuthorizationPolicies
{
    // Policy names
    public const string AdminOnly = nameof(AdminOnly);
    public const string ClinicianOrAdmin = nameof(ClinicianOrAdmin);
    public const string PatientOrClinician = nameof(PatientOrClinician);
    public const string PatientOrAdmin = nameof(PatientOrAdmin);
    public const string AnyAuthenticatedUser = nameof(AnyAuthenticatedUser);

    /// <summary>
    /// Register authorization policies.
    /// Call this in ConfigureServices in Program.cs:
    /// 
    /// <code>
    /// services.AddAuthorization(options =>
    /// {
    ///     AuthorizationPolicies.RegisterPolicies(options);
    /// });
    /// </code>
    /// </summary>
    public static void RegisterPolicies(AuthorizationOptions options)
    {
        // Admin only - system administrators
        options.AddPolicy(AdminOnly, policy =>
            policy.RequireRole("Admin"));

        // Clinician or Admin
        options.AddPolicy(ClinicianOrAdmin, policy =>
            policy.RequireRole("Clinician", "Admin"));

        // Patient or Clinician
        options.AddPolicy(PatientOrClinician, policy =>
            policy.RequireRole("Patient", "Clinician", "Admin"));

        // Patient or Admin
        options.AddPolicy(PatientOrAdmin, policy =>
            policy.RequireRole("Patient", "Admin"));

        // Any authenticated user
        options.AddPolicy(AnyAuthenticatedUser, policy =>
            policy.RequireAuthenticatedUser());
    }
}

/// <summary>
/// Role definitions for EHR platform.
/// </summary>
public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Clinician = "Clinician";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";
    public const string Pharmacist = "Pharmacist";
    public const string Patient = "Patient";
    public const string SystemService = "SystemService";

    /// <summary>
    /// Get all available roles.
    /// </summary>
    public static IEnumerable<string> GetAllRoles()
    {
        yield return Admin;
        yield return Clinician;
        yield return Nurse;
        yield return Receptionist;
        yield return Pharmacist;
        yield return Patient;
        yield return SystemService;
    }

    /// <summary>
    /// Get healthcare provider roles (staff).
    /// </summary>
    public static IEnumerable<string> GetProviderRoles()
    {
        yield return Clinician;
        yield return Nurse;
        yield return Receptionist;
        yield return Pharmacist;
    }
}
