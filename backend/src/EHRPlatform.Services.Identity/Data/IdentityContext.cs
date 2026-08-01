using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Identity.Domain.Entities;

namespace EHRPlatform.Services.Identity.Data
{
    /// <summary>
    /// Service-Specific DbContext for Identity Service
    /// Manages User, Role, Permission entities and their relationships.
    /// This context is ONLY used by the Identity Service.
    /// Other services do NOT reference this context.
    /// </summary>
    public class IdentityContext : DbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        // ─────────────────────────────────────────────────────────────────────────
        // DbSets - Entity Collections
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>System users</summary>
        public DbSet<User> Users { get; set; }

        /// <summary>User roles (Doctor, Patient, Admin, etc.)</summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>Permission definitions (read_patient, create_appointment, etc.)</summary>
        public DbSet<Permission> Permissions { get; set; }

        /// <summary>User-Role assignments (many-to-many)</summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>Role-Permission assignments (many-to-many)</summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>JWT refresh tokens</summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // OnModelCreating - Entity Configuration
        // ─────────────────────────────────────────────────────────────────────────

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─────────────────────────────────────────────────────────────────────
            // User Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                // Properties
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.IsEmailVerified)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasName("IX_Users_Email_Unique");

                entity.HasIndex(e => e.IsActive)
                    .HasName("IX_Users_IsActive");

                entity.HasIndex(e => e.CreatedAt)
                    .HasName("IX_Users_CreatedAt");

                // Soft delete filter
                entity.HasQueryFilter(e => e.DeletedAt == null);

                // Relationships
                entity.HasMany(e => e.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────────────────────────────────
            // Role Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Index for quick role lookup
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasName("IX_Roles_Name_Unique");

                // Relationships
                entity.HasMany(e => e.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Permissions)
                    .WithOne(rp => rp.Role)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────────────────────────────────
            // Permission Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Resource)
                    .HasMaxLength(100);

                entity.Property(e => e.Action)
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Composite index for permission lookup (Resource + Action)
                entity.HasIndex(e => new { e.Resource, e.Action })
                    .HasName("IX_Permissions_Resource_Action");

                // Relationships
                entity.HasMany(e => e.RolePermissions)
                    .WithOne(rp => rp.Permission)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────────────────────────────────
            // UserRole Entity Configuration (Join Table)
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AssignedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Composite unique index to prevent duplicate assignments
                entity.HasIndex(e => new { e.UserId, e.RoleId })
                    .IsUnique()
                    .HasName("UX_UserRoles_User_Role");

                // Indexes for queries
                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserRoles_UserId");

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_UserRoles_RoleId");
            });

            // ─────────────────────────────────────────────────────────────────────
            // RolePermission Entity Configuration (Join Table)
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Composite unique index to prevent duplicate assignments
                entity.HasIndex(e => new { e.RoleId, e.PermissionId })
                    .IsUnique()
                    .HasName("UX_RolePermissions_Role_Permission");

                // Indexes for queries
                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_RolePermissions_RoleId");

                entity.HasIndex(e => e.PermissionId)
                    .HasName("IX_RolePermissions_PermissionId");
            });

            // ─────────────────────────────────────────────────────────────────────
            // RefreshToken Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Unique index on token
                entity.HasIndex(e => e.Token)
                    .IsUnique()
                    .HasName("IX_RefreshTokens_Token_Unique");

                // Indexes for queries
                entity.HasIndex(e => e.UserId)
                    .HasName("IX_RefreshTokens_UserId");

                entity.HasIndex(e => e.ExpiresAt)
                    .HasName("IX_RefreshTokens_ExpiresAt");
            });

            // ─────────────────────────────────────────────────────────────────────
            // Seed Default Data
            // ─────────────────────────────────────────────────────────────────────
            SeedDefaultRoles(modelBuilder);
            SeedDefaultPermissions(modelBuilder);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Helper Methods for Seeding
        // ─────────────────────────────────────────────────────────────────────────

        private static void SeedDefaultRoles(ModelBuilder modelBuilder)
        {
            var adminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var doctorRoleId = Guid.Parse("10000000-0000-0000-0000-000000000002");
            var nurseRoleId = Guid.Parse("10000000-0000-0000-0000-000000000003");
            var patientRoleId = Guid.Parse("10000000-0000-0000-0000-000000000004");

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    Description = "System administrator with full access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = doctorRoleId,
                    Name = "Doctor",
                    Description = "Healthcare provider",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = nurseRoleId,
                    Name = "Nurse",
                    Description = "Nursing staff",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Id = patientRoleId,
                    Name = "Patient",
                    Description = "Patient user",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }

        private static void SeedDefaultPermissions(ModelBuilder modelBuilder)
        {
            var permissions = new[]
            {
                ("read_patient", "Patient", "READ"),
                ("create_patient", "Patient", "CREATE"),
                ("update_patient", "Patient", "UPDATE"),
                ("delete_patient", "Patient", "DELETE"),
                ("read_appointment", "Appointment", "READ"),
                ("create_appointment", "Appointment", "CREATE"),
                ("update_appointment", "Appointment", "UPDATE"),
                ("read_clinical", "Clinical", "READ"),
                ("create_clinical", "Clinical", "CREATE"),
                ("read_prescription", "Prescription", "READ"),
                ("create_prescription", "Prescription", "CREATE"),
                ("read_billing", "Billing", "READ"),
                ("create_billing", "Billing", "CREATE"),
                ("read_audit", "Audit", "READ")
            };

            var permissionsList = permissions
                .Select((p, i) => new Permission
                {
                    Id = new Guid($"20000000-0000-0000-0000-{i:00000000000}"),
                    Name = p.Item1,
                    Resource = p.Item2,
                    Action = p.Item3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            modelBuilder.Entity<Permission>().HasData(permissionsList);
        }
    }
}
