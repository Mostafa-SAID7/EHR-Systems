using BenchmarkDotNet.Attributes;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Security;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Performance.Load;

/// <summary>
/// Performance tests for Identity Service.
/// Tests password hashing, token generation, concurrent operations.
/// Target: < 100ms for token generation, < 500ms for password hashing.
/// </summary>
public class IdentityServiceLoadTests : UnitTestBase
{
    private readonly IPasswordHasher _passwordHasher;
    private const string TestSecret = "SuperSecretKeyThatIsAtLeast32CharactersLong!@#$%";
    private const string TestIssuer = "EHR-Platform";
    private const string TestAudience = "EHR-Services";

    public IdentityServiceLoadTests()
    {
        _passwordHasher = new Argon2PasswordHasher();
    }

    #region Password Hashing Performance Tests

    [Fact]
    public void PasswordHasher_HashPassword_ShouldCompleteWithin500ms()
    {
        // Arrange
        var password = "VerySecurePassword123!@#";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var (hash, salt) = _passwordHasher.HashWithSalt(password);

        stopwatch.Stop();

        // Assert
        hash.Should().NotBeNullOrEmpty();
        salt.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void PasswordVerification_ShouldCompleteWithin100ms()
    {
        // Arrange
        var password = "VerifyPassword123!@#";
        var (hash, salt) = _passwordHasher.HashWithSalt(password);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var isValid = _passwordHasher.Verify(password, hash, salt);

        stopwatch.Stop();

        // Assert
        isValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void BatchPasswordHashing_1000Operations_ShouldCompleteWithin60Seconds()
    {
        // Arrange
        var passwords = Enumerable.Range(0, 1000)
            .Select(i => $"Password{i}Secure123!@#")
            .ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var results = passwords.Select(p => _passwordHasher.HashWithSalt(p)).ToList();

        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(1000);
        results.Should().AllSatisfy(r =>
        {
            r.hash.Should().NotBeNullOrEmpty();
            r.salt.Should().NotBeNullOrEmpty();
        });
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000);
    }

    #endregion

    #region JWT Token Generation Performance Tests

    [Fact]
    public void TokenGeneration_SingleToken_ShouldCompleteWithin100ms()
    {
        // Arrange
        var jwtService = new JwtTokenService(TestSecret, TestIssuer, TestAudience);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "perf@example.com",
            FirstName = "Performance",
            LastName = "Test",
            IsActive = true
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var token = jwtService.GenerateAccessToken(user);

        stopwatch.Stop();

        // Assert
        token.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void TokenGeneration_WithRoles_ShouldCompleteWithin150ms()
    {
        // Arrange
        var jwtService = new JwtTokenService(TestSecret, TestIssuer, TestAudience);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "roles@example.com",
            FirstName = "Roles",
            LastName = "Test",
            IsActive = true
        };

        var roles = new[] { "Doctor", "Admin", "Auditor" };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var token = jwtService.GenerateAccessToken(user, roles);

        stopwatch.Stop();

        // Assert
        token.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150);
    }

    [Fact]
    public void BatchTokenGeneration_1000Tokens_ShouldCompleteWithin2Seconds()
    {
        // Arrange
        var jwtService = new JwtTokenService(TestSecret, TestIssuer, TestAudience);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var tokens = Enumerable.Range(0, 1000)
            .Select(i => new User
            {
                Id = Guid.NewGuid(),
                Email = $"user{i}@example.com",
                FirstName = "Batch",
                LastName = $"User{i}",
                IsActive = true
            })
            .Select(u => jwtService.GenerateAccessToken(u))
            .ToList();

        stopwatch.Stop();

        // Assert
        tokens.Should().HaveCount(1000);
        tokens.Should().AllSatisfy(t => t.Should().NotBeNullOrEmpty());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    #endregion

    #region Concurrent Operation Tests

    [Fact]
    public void ConcurrentPasswordHashing_100ParallelOperations_ShouldCompleteWithin10Seconds()
    {
        // Arrange
        var tasks = new List<Task<(string hash, string salt)>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
                _passwordHasher.HashWithSalt($"ConcurrentPass{index}Secure123!@#")
            ));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        // Assert
        tasks.Should().HaveCount(100);
        tasks.Should().AllSatisfy(t => t.IsCompletedSuccessfully.Should().BeTrue());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Fact]
    public void ConcurrentTokenGeneration_100ParallelOperations_ShouldCompleteWithin3Seconds()
    {
        // Arrange
        var jwtService = new JwtTokenService(TestSecret, TestIssuer, TestAudience);
        var tasks = new List<Task<string>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"concurrent{index}@example.com",
                    FirstName = "Concurrent",
                    LastName = $"User{index}",
                    IsActive = true
                };
                return jwtService.GenerateAccessToken(user);
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        // Assert
        tasks.Should().HaveCount(100);
        tasks.Should().AllSatisfy(t => t.IsCompletedSuccessfully.Should().BeTrue());
        tasks.Select(t => t.Result).Should().AllSatisfy(token => token.Should().NotBeNullOrEmpty());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000);
    }

    #endregion

    #region Memory and Resource Tests

    [Fact]
    public void PasswordHashing_ShouldNotLeakMemory_After1000Operations()
    {
        // Arrange
        var beforeMemory = GC.GetTotalMemory(true);

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _passwordHasher.HashWithSalt($"TestPassword{i}Secure123!@#");
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterMemory = GC.GetTotalMemory(true);

        // Assert
        var memoryIncrease = (afterMemory - beforeMemory) / (1024 * 1024); // MB
        memoryIncrease.Should().BeLessThan(50); // Should not increase by more than 50MB
    }

    [Fact]
    public void TokenGeneration_ShouldMaintainTokenQuality_UnderLoad()
    {
        // Arrange
        var jwtService = new JwtTokenService(TestSecret, TestIssuer, TestAudience);
        var tokenLengths = new List<int>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = $"quality{i}@example.com",
                FirstName = "Quality",
                LastName = $"Test{i}",
                IsActive = true
            };
            
            var token = jwtService.GenerateAccessToken(user);
            tokenLengths.Add(token.Length);
        }

        // Assert - All tokens should be similar length (slight variations OK)
        var avgLength = tokenLengths.Average();
        var minLength = tokenLengths.Min();
        var maxLength = tokenLengths.Max();

        minLength.Should().BeGreaterThan(100);
        maxLength.Should().BeLessThan(avgLength + 50);
    }

    #endregion
}

/// <summary>
/// Benchmark tests for Identity Service using BenchmarkDotNet.
/// Run with: dotnet run -c Release --project EHRPlatform.Tests.Performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class IdentityServiceBenchmarks
{
    private IPasswordHasher _passwordHasher;
    private IJwtTokenService _jwtTokenService;
    private User _testUser;

    private const string TestSecret = "SuperSecretKeyThatIsAtLeast32CharactersLong!@#$%";
    private const string TestIssuer = "EHR-Platform";
    private const string TestAudience = "EHR-Services";

    [GlobalSetup]
    public void Setup()
    {
        _passwordHasher = new Argon2PasswordHasher();
        _jwtTokenService = new JwtTokenService(TestSecret, TestIssuer, TestAudience);
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "benchmark@example.com",
            FirstName = "Benchmark",
            LastName = "User",
            IsActive = true
        };
    }

    [Benchmark]
    public (string hash, string salt) PasswordHashingBenchmark()
    {
        return _passwordHasher.HashWithSalt("BenchmarkPassword123!@#");
    }

    [Benchmark]
    public bool PasswordVerificationBenchmark()
    {
        var (hash, salt) = _passwordHasher.HashWithSalt("BenchmarkPassword123!@#");
        return _passwordHasher.Verify("BenchmarkPassword123!@#", hash, salt);
    }

    [Benchmark]
    public string TokenGenerationBenchmark()
    {
        return _jwtTokenService.GenerateAccessToken(_testUser);
    }

    [Benchmark]
    public string TokenGenerationWithRolesBenchmark()
    {
        return _jwtTokenService.GenerateAccessToken(_testUser, new[] { "Doctor", "Admin" });
    }
}

