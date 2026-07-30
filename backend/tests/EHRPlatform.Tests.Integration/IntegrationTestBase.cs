#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Contexts;
using MediatR;
using Moq;
using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Tests.Integration;

/// <summary>
/// Base class for integration tests using IAsyncLifetime pattern.
/// Provides in-memory database setup and teardown for clean test isolation.
/// Uses SQLite in-memory for more realistic EF Core behavior than InMemoryDatabase.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected DbContextOptions<TestDbContext> DbContextOptions { get; private set; } = null!;
    protected TestDbContext DbContext { get; private set; } = null!;
    protected IMediator Mediator { get; private set; } = null!;
    protected Mock<ITagService> MockTagService { get; private set; } = null!;
    protected Mock<ITagQueryService> MockTagQueryService { get; private set; } = null!;

    /// <summary>
    /// Initialize the test database and dependencies.
    /// Called once per test before test execution.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Create in-memory SQLite database
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        DbContext = new TestDbContext(DbContextOptions);

        // Create database schema
        await DbContext.Database.EnsureCreatedAsync();

        // Setup mocks
        MockTagService = new Mock<ITagService>();
        MockTagQueryService = new Mock<ITagQueryService>();

        // Create service collection with mocks and mediator
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(ApplyTagsCommandHandler).Assembly));
        services.AddSingleton(MockTagService.Object);
        services.AddSingleton(MockTagQueryService.Object);
        services.AddScoped(_ => DbContext);

        var serviceProvider = services.BuildServiceProvider();
        Mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    /// Clean up test database.
    /// Called once per test after test execution.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }
    }

    /// <summary>
    /// Create a tag for testing.
    /// </summary>
    protected Tag CreateTag(
        string name = "TestTag",
        string category = "TestCategory",
        string? description = "Test Description",
        string? colorCode = "#FF5733",
        bool isSystemTag = false,
        string? allowedServices = null)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = name.ToLower().Replace(" ", "-"),
            Category = category,
            Description = description,
            ColorCode = colorCode,
            IsArchived = false,
            UsageCount = 0,
            IsSystemTag = isSystemTag,
            AllowedServices = allowedServices,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return tag;
    }

    /// <summary>
    /// Create a tag association for testing.
    /// </summary>
    protected TagAssociation CreateTagAssociation(
        Guid tagId,
        Guid resourceId,
        string resourceType = "Patient",
        string serviceName = "Patient",
        string? context = null,
        string? appliedBy = null)
    {
        return new TagAssociation
        {
            Id = Guid.NewGuid(),
            TagId = tagId,
            ResourceId = resourceId,
            ResourceType = resourceType,
            ServiceName = serviceName,
            Context = context,
            AppliedBy = appliedBy ?? "test-user",
            AppliedAt = DateTime.UtcNow
        };
    }
}
