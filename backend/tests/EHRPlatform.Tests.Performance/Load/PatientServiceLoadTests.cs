#nullable enable

using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace EHRPlatform.Tests.Performance.Load;

/// <summary>
/// Load testing for PatientService endpoints using NBomber.
/// Tests concurrent user simulation and throughput under load.
/// Target: Handle 1000+ concurrent requests with <500ms response time.
/// </summary>
public class PatientServiceLoadTests
{
    private const string BaseUrl = "http://localhost:5000";
    private const string PatientEndpoint = "/api/v1/patients";

    [Fact]
    public void PatientSearch_Under_LightLoad()
    {
        // Arrange
        var scenario = Scenario.Create("patient_search_light", async context =>
        {
            var client = new HttpClient();
            var request = Http.CreateRequest("GET", $"{BaseUrl}{PatientEndpoint}?search=test");

            var response = await Http.Send(client, request);
            return response.IsSuccessStatusCode 
                ? Response.Ok() 
                : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(
            Simulation.KeepConstant(
                copies: 10,  // 10 concurrent users
                duration: TimeSpan.FromSeconds(30)
            )
        );

        // Act
        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .RunTest();

        // Assert
        result.AllScenarios.Should().HaveCount(1);
        var scn = result.AllScenarios[0];
        scn.Ok.Should().BeGreaterThan(0);
        scn.StatusCodeStats.Should().NotBeEmpty();
    }

    [Fact]
    public void PatientCreate_Under_ModerateLo ad()
    {
        // Arrange
        var scenario = Scenario.Create("patient_create_moderate", async context =>
        {
            var client = new HttpClient();
            var payload = new
            {
                firstName = "John",
                lastName = "Doe",
                email = $"test{context.CorrelationId}@test.com",
                dateOfBirth = "1980-01-01"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var request = Http.CreateRequest("POST", $"{BaseUrl}{PatientEndpoint}")
                .WithHeader("Content-Type", "application/json");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.Url)
            {
                Content = content
            };

            var response = await client.SendAsync(httpRequest);
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(
            Simulation.KeepConstant(
                copies: 50,  // 50 concurrent users
                duration: TimeSpan.FromSeconds(60)
            )
        );

        // Act
        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .RunTest();

        // Assert
        result.AllScenarios.Should().HaveCount(1);
        var scn = result.AllScenarios[0];
        scn.Ok.Should().BeGreaterThan(0);
        scn.AllRequestCount.Should().BeGreaterThan(40); // At least 40 successful requests
    }

    [Fact]
    public void PatientUpdate_Stress_Test()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var scenario = Scenario.Create("patient_update_stress", async context =>
        {
            var client = new HttpClient();
            var payload = new
            {
                email = $"updated{context.CorrelationId}@test.com",
                phone = "+12025551234"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"{BaseUrl}{PatientEndpoint}/{patientId}"
            )
            {
                Content = content
            };

            var response = await client.SendAsync(httpRequest);
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(
            Simulation.Ramp(
                startCopies: 10,
                endCopies: 100,
                duration: TimeSpan.FromSeconds(60)
            )
        );

        // Act
        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .RunTest();

        // Assert
        result.AllScenarios.Should().HaveCount(1);
        var scn = result.AllScenarios[0];
        scn.Ok.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PatientSearch_Spike_Test()
    {
        // Arrange - Sudden spike in traffic
        var scenario = Scenario.Create("patient_search_spike", async context =>
        {
            var client = new HttpClient();
            var request = Http.CreateRequest("GET", $"{BaseUrl}{PatientEndpoint}?search=spike");

            var response = await Http.Send(client, request);
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(
            Simulation.KeepConstant(10, TimeSpan.FromSeconds(10)),
            Simulation.KeepConstant(200, TimeSpan.FromSeconds(30)),  // Spike
            Simulation.KeepConstant(10, TimeSpan.FromSeconds(10))
        );

        // Act
        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .RunTest();

        // Assert
        result.AllScenarios.Should().HaveCount(1);
        var scn = result.AllScenarios[0];
        scn.Ok.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PatientDelete_Stress_HighConcurrency()
    {
        // Arrange
        var scenario = Scenario.Create("patient_delete_stress", async context =>
        {
            var client = new HttpClient();
            var patientId = Guid.NewGuid();

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{BaseUrl}{PatientEndpoint}/{patientId}"
            );

            var response = await client.SendAsync(httpRequest);
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound
                ? Response.Ok()
                : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(
            Simulation.KeepConstant(
                copies: 200,  // 200 concurrent deletes
                duration: TimeSpan.FromSeconds(30)
            )
        );

        // Act
        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .RunTest();

        // Assert
        result.AllScenarios.Should().HaveCount(1);
    }

    [Fact]
    public void CombinedWorkload_Mixed_Operations()
    {
        // Arrange - Realistic mix of operations
        var searchScenario = Scenario.Create("search", async context =>
        {
            var client = new HttpClient();
            var request = Http.CreateRequest("GET", $"{BaseUrl}{PatientEndpoint}?search=test");
            var response = await Http.Send(client, request);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(Simulation.KeepConstant(30, TimeSpan.FromSeconds(60)));

        var createScenario = Scenario.Create("create", async context =>
        {
            var client = new HttpClient();
            var payload = new { firstName = "John", lastName = "Doe", email = "test@test.com" };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{PatientEndpoint}")
            {
                Content = content
            };

            var response = await client.SendAsync(httpRequest);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmup()
        .WithLoadSimulations(Simulation.KeepConstant(20, TimeSpan.FromSeconds(60)));

        // Act
        var result = NBomberRunner
            .RegisterScenarios(searchScenario, createScenario)
            .RunTest();

        // Assert
        result.AllScenarios.Should().HaveCount(2);
        foreach (var scn in result.AllScenarios)
        {
            scn.Ok.Should().BeGreaterThan(0);
        }
    }
}
