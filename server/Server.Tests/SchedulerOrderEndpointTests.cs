using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Server.Tests;

public class SchedulerOrderEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SchedulerOrderEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"test{DateTime.Now.Ticks}@example.com",
            password = "password123"
        });
        
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registerData = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        return registerData.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task PostSchedulerOrder_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var accessToken = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var request = new
        {
            tasks = new[]
            {
                new { title = "Design API", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new string[0] },
                new { title = "Implement Backend", estimatedHours = 12, dueDate = "2025-10-28", dependencies = new[] { "Design API" } },
                new { title = "Build Frontend", estimatedHours = 10, dueDate = "2025-10-30", dependencies = new[] { "Design API" } },
                new { title = "End-to-End Test", estimatedHours = 8, dueDate = "2025-10-31", dependencies = new[] { "Implement Backend", "Build Frontend" } }
            },
            strategy = 0 // DepsDueSjf = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scheduler/order", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected OK but got {response.StatusCode}. Content: {errorContent}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("recommendedOrder", out var order));
        Assert.True(result.TryGetProperty("strategyUsed", out var strategy));
        Assert.Equal("DepsDueSjf", strategy.GetString());
        
        var orderArray = order.EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Equal("Design API", orderArray[0]);
        Assert.Contains("Implement Backend", orderArray);
        Assert.Contains("Build Frontend", orderArray);
        Assert.Equal("End-to-End Test", orderArray[3]);
    }

    [Fact]
    public async Task PostSchedulerOrder_WithDuplicateTitles_ReturnsBadRequest()
    {
        // Arrange
        var accessToken = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var request = new
        {
            tasks = new[]
            {
                new { title = "Task1", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new string[0] },
                new { title = "Task1", estimatedHours = 3, dueDate = "2025-10-26", dependencies = new string[0] }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scheduler/order", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("title", out var title));
        Assert.Equal("Invalid request", title.GetString());
    }

    [Fact]
    public async Task PostSchedulerOrder_WithCircularDependency_ReturnsUnprocessableEntity()
    {
        // Arrange
        var accessToken = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var request = new
        {
            tasks = new[]
            {
                new { title = "A", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new[] { "B" } },
                new { title = "B", estimatedHours = 3, dueDate = "2025-10-26", dependencies = new[] { "C" } },
                new { title = "C", estimatedHours = 8, dueDate = "2025-10-27", dependencies = new[] { "A" } }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scheduler/order", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("title", out var title));
        Assert.Equal("Scheduling failed", title.GetString());
    }

    [Fact]
    public async Task PostSchedulerOrder_WithUnknownDependency_ReturnsBadRequest()
    {
        // Arrange
        var accessToken = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var request = new
        {
            tasks = new[]
            {
                new { title = "Task1", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new[] { "UnknownTask" } }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scheduler/order", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("title", out var title));
        Assert.Equal("Invalid request", title.GetString());
    }

    [Fact]
    public async Task PostSchedulerOrder_WithSelfDependency_ReturnsBadRequest()
    {
        // Arrange
        var accessToken = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var request = new
        {
            tasks = new[]
            {
                new { title = "Task1", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new[] { "Task1" } }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scheduler/order", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("title", out var title));
        Assert.Equal("Invalid request", title.GetString());
    }

    [Fact]
    public async Task PostSchedulerOrder_WithDifferentStrategies_ReturnsCorrectOrder()
    {
        // Arrange
        var accessToken = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var tasks = new[]
        {
            new { title = "TaskA", estimatedHours = 10, dueDate = "2025-10-30", dependencies = new string[0] },
            new { title = "TaskB", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new string[0] },
            new { title = "TaskC", estimatedHours = 8, dueDate = "2025-10-30", dependencies = new string[0] }
        };

        // Test DepsDueSjf strategy
        var requestSjf = new { tasks, strategy = 0 }; // DepsDueSjf = 0
        var responseSjf = await _client.PostAsJsonAsync("/api/scheduler/order", requestSjf);
        Assert.Equal(HttpStatusCode.OK, responseSjf.StatusCode);
        var resultSjf = await responseSjf.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DepsDueSjf", resultSjf.GetProperty("strategyUsed").GetString());

        // Test DepsDueFifo strategy
        var requestFifo = new { tasks, strategy = 1 }; // DepsDueFifo = 1
        var responseFifo = await _client.PostAsJsonAsync("/api/scheduler/order", requestFifo);
        Assert.Equal(HttpStatusCode.OK, responseFifo.StatusCode);
        var resultFifo = await responseFifo.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DepsDueFifo", resultFifo.GetProperty("strategyUsed").GetString());

        // Test DepsOnly strategy
        var requestDeps = new { tasks, strategy = 2 }; // DepsOnly = 2
        var responseDeps = await _client.PostAsJsonAsync("/api/scheduler/order", requestDeps);
        Assert.Equal(HttpStatusCode.OK, responseDeps.StatusCode);
        var resultDeps = await responseDeps.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DepsOnly", resultDeps.GetProperty("strategyUsed").GetString());
    }

    [Fact]
    public async Task PostSchedulerOrder_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new
        {
            tasks = new[]
            {
                new { title = "Task1", estimatedHours = 5, dueDate = "2025-10-25", dependencies = new string[0] }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scheduler/order", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}