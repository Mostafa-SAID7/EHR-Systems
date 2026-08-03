namespace Identity.Tests.Integration.Controllers;

using FluentAssertions;
using Identity.Contracts.Requests;
using Identity.Contracts.Responses;
using Identity.Tests.Integration.Fixtures;
using System.Net;
using System.Net.Http.Json;
using Xunit;

/// <summary>
/// Integration tests for the AuthController
/// </summary>
public sealed class AuthControllerIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;
    private IdentityWebApplicationFactory _factory = null!;

    /// <summary>
    /// Initializes a new instance of the AuthControllerIntegrationTests class
    /// </summary>
    public AuthControllerIntegrationTests()
    {
        _databaseFixture = new DatabaseFixture();
    }

    /// <summary>
    /// Initializes the test fixture
    /// </summary>
    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        _factory = new IdentityWebApplicationFactory(_databaseFixture.DbContext);
    }

    /// <summary>
    /// Disposes the test fixture
    /// </summary>
    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await _databaseFixture.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest("test@example.com", "SecurePass123!");

        // TODO: Create user first using application layer

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<AuthResponse>();
        content.Should().NotBeNull();
        content.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest("nonexistent@example.com", "SecurePass123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest("test@example.com", "WrongPassword123!");

        // TODO: Create user first using application layer

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
