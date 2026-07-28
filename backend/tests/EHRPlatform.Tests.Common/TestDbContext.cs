#nullable enable

using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Patient.Domain.Entities;

namespace EHRPlatform.Tests.Common;

/// <summary>
/// Test database context for integration tests.
/// Includes entities from all microservices for comprehensive testing.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Patient configuration
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.MRN).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
            entity.Property(e => e.Gender).HasMaxLength(1);

            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.MRN);
            entity.HasIndex(e => e.Phone);
        });
    }
}
