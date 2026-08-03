using System;
using System.Collections.Generic;

namespace Identity.Infrastructure.Authorization;

/// <summary>
/// Role definitions for Identity service and EHR platform.
/// Single responsibility: Role constants and role management.
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
