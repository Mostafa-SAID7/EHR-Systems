namespace Identity.Persistence.DbContexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory for creating IdentityDbContext instances during design time
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    /// <summary>
    /// Creates a new IdentityDbContext instance
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>A new IdentityDbContext instance</returns>
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost,1433;Database=EHRIdentity;User Id=sa;Password=P@ssw0rd123!;Encrypt=false;TrustServerCertificate=true;";

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(connectionString);

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
