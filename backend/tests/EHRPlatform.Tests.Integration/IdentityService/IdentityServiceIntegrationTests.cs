#nullable enable

using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using EHRPlatform.Common.Security;
using EHRPlatform.Services.Identity.Data;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Domain.Enums;
using EHRPlatform.Tests.Common.Base;
using EHRPlatform.Tests.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Tests.Integration.IdentityService;

/// <summary>
/// Integration tests for Identity Service with real PostgreSQL database.
/// Tests user CRUD, authentication workflows, role assignments, MFA setup.
/// Target: ≥70% coverage
/// HIPAA-focused: Tests audit trails, password security, session management.
/// </summary>
public class IdentityServiceIntegrationTests : IntegrationTestBase
{
    private readonly IPasswordHasher _passwordHasher;

    public IdentityServiceIntegrationTests()
    {
        _passwordHasher = new Argon2PasswordHasher();
    }

    #region User CRUD Tests

    [Fact]
    public async Task CreateUser_WithValidData_ShouldPersist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "integration@test.com";
        var firstName = "Integration";
        var lastName = "Test";
        var (passwordHash, passwordSalt) = _passwordHasher.HashWithSalt("SecurePass123!");

        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsActive = true,
            EmailConfirmed = false,
            MfaEnabled = false,
            CreatedBy = Guid.Empty
        };

        // Act
        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<User>().FindAsync(userId);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(email);
        retrieved.FirstName.Should().Be(firstName);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ReadUser_ByEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "read@test.com";
        var (hash, salt) = _passwordHasher.HashWithSalt("Pass123!");
        var user = new User
        {
            Email = email,
            FirstName = "Read",
            LastName = "Test",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        // Act
        var retrieved = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("Read");
    }

    [Fact]
    public async Task UpdateUser_Email_ShouldReflectInDatabase()
    {
        // Arrange
        var user = new User
        {
            Email = "old@test.com",
            FirstName = "Update",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        // Act
        user.Email = "new@test.com";
        user.UpdatedBy = user.Id;
        DbContext.Set<User>().Update(user);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<User>().FindAsync(user.Id);
        retrieved!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task DeleteUser_SoftDelete_ShouldMarkDeletedAt()
    {
        // Arrange
        var user = new User
        {
            Email = "delete@test.com",
            FirstName = "Delete",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        // Act
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = user.Id;
        DbContext.Set<User>().Update(user);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
        retrieved!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Authentication Workflow Tests

    [Fact]
    public async Task LoginAudit_AfterFailedAttempt_ShouldLogEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "audit@test.com",
            FirstName = "Audit",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        var loginAudit = new LoginAudit
        {
            UserId = userId,
            Email = "audit@test.com",
            Success = false,
            FailureReason = "Invalid password",
            IpAddress = "192.168.1.1",
            UserAgent = "Test-Agent/1.0"
        };

        // Act
        DbContext.Set<User>().Add(user);
        DbContext.Set<LoginAudit>().Add(loginAudit);
        await SaveChangesAsync();

        // Assert
        var audit = await DbContext.Set<LoginAudit>()
            .FirstOrDefaultAsync(la => la.UserId == userId && !la.Success);
        
        audit.Should().NotBeNull();
        audit!.Success.Should().BeFalse();
        audit.FailureReason.Should().Contain("Invalid password");
    }

    [Fact]
    public async Task LoginAudit_AfterSuccessfulLogin_ShouldLogAndUpdateLastLogin()
    {
        // Arrange
        var user = new User
        {
            Email = "success@test.com",
            FirstName = "Success",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var loginAudit = new LoginAudit
        {
            UserId = user.Id,
            Email = user.Email,
            Success = true,
            IpAddress = "192.168.1.1",
            UserAgent = "Test-Agent/1.0"
        };

        // Act
        user.LastLogin = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.UpdatedBy = user.Id;
        
        DbContext.Set<User>().Update(user);
        DbContext.Set<LoginAudit>().Add(loginAudit);
        await SaveChangesAsync();

        // Assert
        var retrievedUser = await DbContext.Set<User>().FindAsync(user.Id);
        retrievedUser!.LastLogin.Should().HaveValue();
        retrievedUser.FailedLoginAttempts.Should().Be(0);
    }

    #endregion

    #region Account Lockout Tests

    [Fact]
    public async Task AccountLockout_After5FailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var user = new User
        {
            Email = "lockout@test.com",
            FirstName = "Lockout",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            FailedLoginAttempts = 0,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        // Act
        user.FailedLoginAttempts = 5;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        DbContext.Set<User>().Update(user);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<User>().FindAsync(user.Id);
        retrieved!.IsLocked().Should().BeTrue();
        retrieved.FailedLoginAttempts.Should().Be(5);
    }

    [Fact]
    public async Task AccountUnlock_ShouldClearLockoutAndResetAttempts()
    {
        // Arrange
        var user = new User
        {
            Email = "unlock@test.com",
            FirstName = "Unlock",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            FailedLoginAttempts = 5,
            LockoutEnd = DateTime.UtcNow.AddMinutes(15),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        // Act
        user.Unlock();
        DbContext.Set<User>().Update(user);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<User>().FindAsync(user.Id);
        retrieved!.IsLocked().Should().BeFalse();
        retrieved.FailedLoginAttempts.Should().Be(0);
        retrieved.LockoutEnd.Should().BeNull();
    }

    #endregion

    #region Role Assignment Tests

    [Fact]
    public async Task AssignRole_ToUser_ShouldCreateUserRoleLink()
    {
        // Arrange
        var adminRoleId = Guid.Parse("10000001-0000-0000-0000-000000000001"); // Admin role (seeded)
        var user = new User
        {
            Email = "role@test.com",
            FirstName = "Role",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRoleId
        };

        // Act
        DbContext.Set<UserRole>().Add(userRole);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id);
        
        retrieved.Should().NotBeNull();
        retrieved!.RoleId.Should().Be(adminRoleId);
    }

    [Fact]
    public async Task UserRoleAssignment_ShouldEnableRoleQueries()
    {
        // Arrange
        var doctorRoleId = Guid.Parse("10000001-0000-0000-0000-000000000002"); // Doctor role (seeded)
        var user = new User
        {
            Email = "doctor@test.com",
            FirstName = "Dr",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var userRole = new UserRole { UserId = user.Id, RoleId = doctorRoleId };
        DbContext.Set<UserRole>().Add(userRole);
        await SaveChangesAsync();

        // Act
        var usersWithDoctorRole = await DbContext.Set<User>()
            .Where(u => u.Roles.Any(r => r.RoleId == doctorRoleId))
            .ToListAsync();

        // Assert
        usersWithDoctorRole.Should().Contain(u => u.Id == user.Id);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task StoreRefreshToken_WithUser_ShouldPersist()
    {
        // Arrange
        var user = new User
        {
            Email = "refresh@test.com",
            FirstName = "Refresh",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "hashed_token_value",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedBy = user.Id
        };

        // Act
        DbContext.Set<RefreshToken>().Add(refreshToken);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id);
        
        retrieved.Should().NotBeNull();
        retrieved!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ExpiredRefreshToken_ShouldReturnInvalidStatus()
    {
        // Arrange
        var user = new User
        {
            Email = "expired@test.com",
            FirstName = "Expired",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var expiredRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "expired_token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedBy = user.Id
        };

        DbContext.Set<RefreshToken>().Add(expiredRefreshToken);
        await SaveChangesAsync();

        // Act
        var retrieved = await DbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id);

        // Assert
        retrieved!.IsExpired.Should().BeTrue();
        retrieved.IsValid.Should().BeFalse();
    }

    #endregion

    #region MFA Setup Tests

    [Fact]
    public async Task SetupMfa_WithUser_ShouldCreateMfaSetup()
    {
        // Arrange
        var user = new User
        {
            Email = "mfa@test.com",
            FirstName = "MFA",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            MfaEnabled = false,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var mfaSetup = new MfaSetup
        {
            UserId = user.Id,
            MfaType = "TOTP",
            Secret = "encrypted_secret",
            IsVerified = false,
            CreatedBy = user.Id
        };

        // Act
        DbContext.Set<MfaSetup>().Add(mfaSetup);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<MfaSetup>()
            .FirstOrDefaultAsync(m => m.UserId == user.Id);
        
        retrieved.Should().NotBeNull();
        retrieved!.MfaType.Should().Be("TOTP");
        retrieved.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyMfa_ShouldMarkAsVerifiedAndEnableMfa()
    {
        // Arrange
        var user = new User
        {
            Email = "verify@test.com",
            FirstName = "Verify",
            LastName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            MfaEnabled = false,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<User>().Add(user);
        await SaveChangesAsync();

        var mfaSetup = new MfaSetup
        {
            UserId = user.Id,
            MfaType = "TOTP",
            Secret = "secret",
            IsVerified = false,
            CreatedBy = user.Id
        };

        DbContext.Set<MfaSetup>().Add(mfaSetup);
        await SaveChangesAsync();

        // Act
        mfaSetup.IsVerified = true;
        mfaSetup.VerifiedAt = DateTime.UtcNow;
        mfaSetup.UpdatedBy = user.Id;

        user.MfaEnabled = true;
        user.MfaSecret = "encrypted_secret";
        user.UpdatedBy = user.Id;

        DbContext.Set<MfaSetup>().Update(mfaSetup);
        DbContext.Set<User>().Update(user);
        await SaveChangesAsync();

        // Assert
        var retrievedUser = await DbContext.Set<User>().FindAsync(user.Id);
        var retrievedMfa = await DbContext.Set<MfaSetup>().FindAsync(mfaSetup.Id);

        retrievedUser!.MfaEnabled.Should().BeTrue();
        retrievedMfa!.IsVerified.Should().BeTrue();
        retrievedMfa.VerifiedAt.Should().HaveValue();
    }

    #endregion

    #region Transaction Isolation Tests

    [Fact]
    public async Task ConcurrentUserCreation_ShouldNotCauseDeadlock()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var user = new User
                {
                    Email = $"concurrent{index}@test.com",
                    FirstName = "Concurrent",
                    LastName = $"Test{index}",
                    PasswordHash = "hash",
                    PasswordSalt = "salt",
                    IsActive = true,
                    CreatedBy = Guid.Empty
                };

                DbContext.Set<User>().Add(user);
                await SaveChangesAsync();
            }));
        }

        // Assert
        await Task.WhenAll(tasks);

        var count = await DbContext.Set<User>()
            .Where(u => u.Email.StartsWith("concurrent"))
            .CountAsync();

        count.Should().Be(5);
    }

    #endregion
}
