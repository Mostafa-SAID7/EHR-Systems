namespace Identity.Tests.Integration.Fixtures;

using Identity.Persistence.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Custom web application factory for integration testing
/// </summary>
public sealed class IdentityWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IdentityDbContext? _dbContext;

    /// <summary>
    /// Initializes a new instance of the IdentityWebApplicationFactory class
    /// </summary>
    /// <param name="dbContext">Optional existing database context</param>
    public IdentityWebApplicationFactory(IdentityDbContext? dbContext = null)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Configures the web host for testing
    /// </summary>
    /// <param name="builder">The web host builder</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase("IdentityTestDb");
            });

            // If a specific context is provided, use it
            if (_dbContext != null)
            {
                services.AddScoped(_ => _dbContext);
            }
        });
    }
}
