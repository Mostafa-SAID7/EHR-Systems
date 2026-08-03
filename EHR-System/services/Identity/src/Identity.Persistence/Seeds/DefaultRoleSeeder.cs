namespace Identity.Persistence.Seeds;

using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Seeds default roles into the database
/// </summary>
public static class DefaultRoleSeeder
{
    /// <summary>
    /// Seeds the default roles
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void SeedDefaultRoles(this ModelBuilder modelBuilder)
    {
        var roles = new[]
        {
            Role.Create("Admin", RoleType.Admin, "System administrator with full access"),
            Role.Create("Provider", RoleType.Provider, "Healthcare provider (doctor, nurse, etc.)"),
            Role.Create("Patient", RoleType.Patient, "Patient receiving healthcare services"),
            Role.Create("Staff", RoleType.Staff, "Administrative and support staff"),
            Role.Create("Manager", RoleType.Manager, "Clinic/Hospital management"),
            Role.Create("Billing", RoleType.Billing, "Billing and finance staff"),
            Role.Create("Viewer", RoleType.Viewer, "Read-only access role")
        };

        modelBuilder.Entity<Role>().HasData(roles);
    }
}
