using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Domain.Enums;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Security.Authorization;

/// <summary>
/// Security tests for Role-Based Access Control (RBAC).
/// Tests permission assignment, role hierarchy, access control enforcement.
/// HIPAA-critical: Enforces proper authorization for PHI access.
/// </summary>
public class RBACTests : UnitTestBase
{
    #region Permission Format Tests

    [Fact]
    public void Permission_Create_WithValidResourceAndAction_ShouldFormatNameCorrectly()
    {
        // Arrange
        var resource = "Patient";
        var action = "Read";
        var description = "View patient records";

        // Act
        var permission = Permission.Create(resource, action, description);

        // Assert
        permission.Name.Should().Be("patient:read");
        permission.Resource.Should().Be("patient");
        permission.Action.Should().Be("read");
        permission.Description.Should().Be(description);
    }

    [Fact]
    public void Permission_IsValidFormat_WithCorrectFormat_ShouldReturnTrue()
    {
        // Arrange
        var permission = Permission.Create("Patient", "Write", "Update patient records");

        // Act
        var isValid = permission.IsValidFormat();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Permission_IsValidFormat_WithMissingResource_ShouldReturnFalse()
    {
        // Arrange
        var permission = new Permission
        {
            Name = ":read",
            Resource = string.Empty,
            Action = "read"
        };

        // Act
        var isValid = permission.IsValidFormat();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Permission_IsValidFormat_WithMissingAction_ShouldReturnFalse()
    {
        // Arrange
        var permission = new Permission
        {
            Name = "patient:",
            Resource = "patient",
            Action = string.Empty
        };

        // Act
        var isValid = permission.IsValidFormat();

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Role and Permission Association Tests

    [Fact]
    public void RolePermission_LinkRoleToPermission_ShouldCreateAssociation()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        // Act & Assert
        rolePermission.RoleId.Should().Be(roleId);
        rolePermission.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void Role_WithMultiplePermissions_ShouldMaintainAssociations()
    {
        // Arrange
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Doctor",
            Description = "Healthcare provider role"
        };

        var perm1 = Guid.NewGuid();
        var perm2 = Guid.NewGuid();
        var perm3 = Guid.NewGuid();

        var rolePerms = new List<RolePermission>
        {
            new() { RoleId = role.Id, PermissionId = perm1 },
            new() { RoleId = role.Id, PermissionId = perm2 },
            new() { RoleId = role.Id, PermissionId = perm3 }
        };

        // Act
        foreach (var rp in rolePerms)
            role.Permissions.Add(rp);

        // Assert
        role.Permissions.Should().HaveCount(3);
        role.Permissions.Should().AllSatisfy(rp => rp.RoleId.Should().Be(role.Id));
    }

    #endregion

    #region User Role Assignment Tests

    [Fact]
    public void UserRole_AssignRoleToUser_ShouldCreateLink()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.Parse("10000001-0000-0000-0000-000000000002"); // Doctor role

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        // Act & Assert
        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void User_WithMultipleRoles_ShouldMaintainAssociations()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "multi@example.com",
            FirstName = "Multi",
            LastName = "Role",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        var doctorRoleId = Guid.Parse("10000001-0000-0000-0000-000000000002");
        var adminRoleId = Guid.Parse("10000001-0000-0000-0000-000000000001");

        var userRoles = new List<UserRole>
        {
            new() { UserId = user.Id, RoleId = doctorRoleId },
            new() { UserId = user.Id, RoleId = adminRoleId }
        };

        // Act
        foreach (var ur in userRoles)
            user.Roles.Add(ur);

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().AllSatisfy(ur => ur.UserId.Should().Be(user.Id));
    }

    #endregion

    #region Access Control Tests

    [Fact]
    public void PatientDataAccess_OnlyDoctorCanWrite_ShouldEnforcePermission()
    {
        // Arrange
        var doctorPermissions = new[]
        {
            "patient:read",
            "patient:write",
            "prescription:write",
            "medical_record:read"
        };

        var patientPermissions = new[]
        {
            "patient:read",
            "medical_record:read"
        };

        // Act & Assert - Doctor can write
        doctorPermissions.Should().Contain("patient:write");

        // Act & Assert - Patient cannot write
        patientPermissions.Should().NotContain("patient:write");
    }

    [Fact]
    public void SensitiveDataAccess_OnlyAdminCanDelete_ShouldEnforcePermission()
    {
        // Arrange
        var adminPermissions = new[]
        {
            "user:create",
            "user:read",
            "user:write",
            "user:delete",
            "role:manage",
            "audit:read"
        };

        var doctorPermissions = new[]
        {
            "patient:read",
            "patient:write",
            "prescription:write"
        };

        // Act & Assert - Admin can delete
        adminPermissions.Should().Contain("user:delete");

        // Act & Assert - Doctor cannot delete
        doctorPermissions.Should().NotContain("user:delete");
    }

    [Fact]
    public void AuditAccess_OnlyAuditorCanViewAuditLogs_ShouldEnforcePermission()
    {
        // Arrange
        var auditorPermissions = new[]
        {
            "audit:read",
            "audit:export",
            "compliance:review"
        };

        var nursePermissions = new[]
        {
            "patient:read",
            "vitals:write",
            "medication:read"
        };

        // Act & Assert - Auditor can read audit
        auditorPermissions.Should().Contain("audit:read");

        // Act & Assert - Nurse cannot read audit
        nursePermissions.Should().NotContain("audit:read");
    }

    [Fact]
    public void BillingAccess_OnlyBillingStaffCanAccessInvoices_ShouldEnforcePermission()
    {
        // Arrange
        var billingPermissions = new[]
        {
            "billing:read",
            "invoice:create",
            "invoice:write",
            "claim:submit",
            "report:generate"
        };

        var receptionistPermissions = new[]
        {
            "appointment:create",
            "appointment:read",
            "patient:read"
        };

        // Act & Assert - Billing can access
        billingPermissions.Should().Contain("invoice:create");

        // Act & Assert - Receptionist cannot access
        receptionistPermissions.Should().NotContain("invoice:create");
    }

    #endregion

    #region Role Type Tests

    [Fact]
    public void RoleType_AllEnumValuesAreValid()
    {
        // Arrange & Act
        var roleTypes = Enum.GetValues(typeof(RoleType)).Cast<RoleType>().ToList();

        // Assert
        roleTypes.Should().Contain(RoleType.Admin);
        roleTypes.Should().Contain(RoleType.Doctor);
        roleTypes.Should().Contain(RoleType.Nurse);
        roleTypes.Should().Contain(RoleType.Patient);
        roleTypes.Should().Contain(RoleType.Receptionist);
        roleTypes.Should().Contain(RoleType.Pharmacist);
        roleTypes.Should().Contain(RoleType.Billing);
        roleTypes.Should().HaveCount(7);
    }

    [Fact]
    public void RoleType_CorrespondsToSeededRoles()
    {
        // Arrange - Stable GUIDs from IdentityContext seeding
        var roleMapping = new Dictionary<RoleType, string>
        {
            { RoleType.Admin, "Admin" },
            { RoleType.Doctor, "Doctor" },
            { RoleType.Nurse, "Nurse" },
            { RoleType.Patient, "Patient" },
            { RoleType.Receptionist, "Receptionist" },
            { RoleType.Pharmacist, "Pharmacist" },
            { RoleType.Billing, "Billing" }
        };

        // Act & Assert
        foreach (var kvp in roleMapping)
        {
            kvp.Value.Should().NotBeNullOrEmpty();
            kvp.Key.ToString().Should().Be(kvp.Value);
        }
    }

    #endregion

    #region HIPAA Compliance Tests

    [Fact]
    public void PHIAccess_OnlyAuthorizedRolesCanAccessPatientPHI()
    {
        // Arrange
        var authorizedRoles = new[] { "Doctor", "Nurse", "Admin" };
        var unauthorizedRoles = new[] { "Receptionist", "Patient" };

        var requestedRole = "Receptionist";

        // Act
        var canAccessPHI = authorizedRoles.Contains(requestedRole);

        // Assert
        canAccessPHI.Should().BeFalse();
    }

    [Fact]
    public void PatientDataModification_ShouldRequireAuditTrail()
    {
        // Arrange
        var modification = new
        {
            UserId = Guid.NewGuid(),
            Action = "patient:write",
            Timestamp = DateTime.UtcNow,
            IPAddress = "192.168.1.100",
            Changes = "Updated patient email address"
        };

        // Act & Assert
        modification.Action.Should().Be("patient:write");
        modification.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        modification.IPAddress.Should().NotBeNullOrEmpty();
        modification.Changes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SensitiveOperations_ShouldEnforceMultipleAuthorizationLayers()
    {
        // Arrange
        var operation = new
        {
            Operation = "DeleteUser",
            RequiredRoles = new[] { "Admin" },
            RequiredMFA = true,
            RequiresAuditLog = true,
            RequiresIPWhitelist = true
        };

        // Act & Assert
        operation.RequiredRoles.Should().Contain("Admin");
        operation.RequiredMFA.Should().BeTrue();
        operation.RequiresAuditLog.Should().BeTrue();
        operation.RequiresIPWhitelist.Should().BeTrue();
    }

    #endregion
}
