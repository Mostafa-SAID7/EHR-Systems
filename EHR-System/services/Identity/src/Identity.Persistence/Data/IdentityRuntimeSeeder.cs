#nullable enable

using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Persistence.Data;

/// <summary>
/// Idempotent runtime seeder for the Identity service.
/// Safe to call on every startup â€” all operations are INSERT-IF-NOT-EXISTS.
///
/// Seeds:
///   1. All roles defined in <see cref="RoleType"/> (fills gaps left by static seed)
///   2. Core permissions (resource:action pairs)
///   3. Admin superuser with a hashed password
/// </summary>
public static class IdentityRuntimeSeeder
{
    /// <summary>
    /// Role descriptions keyed by <see cref="RoleType"/> name.
    /// </summary>
    private static readonly Dictionary<string, string> RoleDescriptions = new()
    {
        [nameof(RoleType.Admin)]         = "System administrator with full access",
        [nameof(RoleType.Doctor)]        = "Licensed healthcare provider",
        [nameof(RoleType.Nurse)]         = "Nursing and care staff",
        [nameof(RoleType.Patient)]       = "Registered patient account",
        [nameof(RoleType.Receptionist)]  = "Front-desk and scheduling staff",
        [nameof(RoleType.Pharmacist)]    = "Pharmacy and medication management",
        [nameof(RoleType.Billing)]       = "Billing and insurance claims"
    };

    /// <summary>
    /// Core permissions every fresh deployment should have.
    /// Format: (resource, action, description)
    /// </summary>
    private static readonly (string Resource, string Action, string Description)[] CorePermissions =
    [
        ("patient",      "read",   "View patient demographics and summary"),
        ("patient",      "write",  "Create and update patient records"),
        ("patient",      "delete", "Soft-delete patient records"),
        ("appointment",  "read",   "View appointments"),
        ("appointment",  "write",  "Create and update appointments"),
        ("prescription", "read",   "View prescriptions"),
        ("prescription", "write",  "Create and update prescriptions"),
        ("billing",      "read",   "View billing records"),
        ("billing",      "write",  "Create and update billing records"),
        ("audit",        "read",   "View audit logs"),
        ("user",         "read",   "View user accounts"),
        ("user",         "write",  "Create and update user accounts"),
        ("user",         "delete", "Deactivate user accounts"),
        ("role",         "read",   "View roles and permissions"),
        ("role",         "write",  "Assign and revoke roles")
    ];

    /// <summary>
    /// Run all seed steps.  Logs what was inserted; silent when already seeded.
    /// </summary>
    public static async Task SeedAsync(
        IdentityContext     db,
        IPasswordHasher     hasher,
        ILogger             logger,
        CancellationToken   ct = default)
    {
        await SeedRolesAsync(db, logger, ct);
        await SeedPermissionsAsync(db, logger, ct);
        await SeedAdminUserAsync(db, hasher, logger, ct);
    }

    // â”€â”€ Step 1: Roles â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static async Task SeedRolesAsync(
        IdentityContext   db,
        ILogger           logger,
        CancellationToken ct)
    {
        var existingNames = await db.Roles
            .Select(r => r.Name)
            .ToListAsync(ct);

        var toAdd = RoleDescriptions
            .Where(kv => !existingNames.Contains(kv.Key))
            .Select(kv => new Role { Name = kv.Key, Description = kv.Value })
            .ToList();

        if (toAdd.Count == 0) return;

        db.Roles.AddRange(toAdd);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} role(s): {Names}",
            toAdd.Count, string.Join(", ", toAdd.Select(r => r.Name)));
    }

    // â”€â”€ Step 2: Permissions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static async Task SeedPermissionsAsync(
        IdentityContext   db,
        ILogger           logger,
        CancellationToken ct)
    {
        var existingNames = await db.Permissions
            .Select(p => p.Name)
            .ToListAsync(ct);

        var toAdd = CorePermissions
            .Select(p => Permission.Create(p.Resource, p.Action, p.Description))
            .Where(p => !existingNames.Contains(p.Name))
            .ToList();

        if (toAdd.Count == 0) return;

        db.Permissions.AddRange(toAdd);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} permission(s)", toAdd.Count);
    }

    // â”€â”€ Step 3: Admin superuser â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private const string AdminEmail    = "admin@ehrs.local";
    private const string AdminPassword = "Admin@EHR2026!";   // Change on first login

    private static async Task SeedAdminUserAsync(
        IdentityContext   db,
        IPasswordHasher   hasher,
        ILogger           logger,
        CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == AdminEmail, ct)) return;

        var (hash, salt) = hasher.HashWithSalt(AdminPassword);

        var admin = new User
        {
            Email          = AdminEmail,
            FirstName      = "System",
            LastName       = "Admin",
            PasswordHash   = hash,
            PasswordSalt   = salt,
            IsActive       = true,
            EmailConfirmed = true,
            MfaEnabled     = false,
            CreatedBy      = Guid.Empty   // system
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync(ct);

        // Assign the Admin role
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == nameof(RoleType.Admin), ct);
        if (adminRole != null)
        {
            db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "Seeded admin user {Email} â€” change this password immediately in production",
            AdminEmail);
    }
}



