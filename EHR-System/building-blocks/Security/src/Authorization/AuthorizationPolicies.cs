using Microsoft.AspNetCore.Authorization;

namespace EHRPlatform.Security.Authorization;

/// <summary>
/// Authorization policy definitions for EHR platform.
/// Single responsibility: Policy registration and definitions.
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
