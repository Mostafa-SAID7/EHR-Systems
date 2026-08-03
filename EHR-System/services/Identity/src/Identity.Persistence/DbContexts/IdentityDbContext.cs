namespace Identity.Persistence.DbContexts;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core DbContext for the Identity service
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the IdentityDbContext class
    /// </summary>
    /// <param name="options">The DbContext options</param>
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users DbSet
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the Roles DbSet
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// Gets or sets the UserRoles DbSet
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// Configures the database model
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
