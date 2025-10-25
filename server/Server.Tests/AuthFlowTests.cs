using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Server.Tests;

public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthFlowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_CreateProject_AddTask_UpdateTask_DeleteTask_DeleteProject_Flow()
    {
        // Register a new user
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"test{DateTime.Now.Ticks}@example.com",
            password = "password123"
        });
        
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registerData = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = registerData.GetProperty("accessToken").GetString();
        var refreshToken = registerData.GetProperty("refreshToken").GetString();
        
        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);

        // Create a project
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        
        var createProjectResponse = await _client.PostAsJsonAsync("/api/projects", new
        {
            title = "Test Project",
            description = "Test Description"
        });
        
        Assert.Equal(HttpStatusCode.Created, createProjectResponse.StatusCode);
        var projectData = await createProjectResponse.Content.ReadFromJsonAsync<JsonElement>();
        var projectId = projectData.GetProperty("id").GetString();
        Assert.NotNull(projectId);

        // Add a task
        var createTaskResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", new
        {
            title = "Test Task",
            dueDate = "2024-12-31T23:59:59"
        });
        
        Assert.Equal(HttpStatusCode.Created, createTaskResponse.StatusCode);
        var taskData = await createTaskResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = taskData.GetProperty("id").GetString();
        Assert.NotNull(taskId);

        // Get project with tasks
        var getProjectResponse = await _client.GetAsync($"/api/projects/{projectId}");
        Assert.Equal(HttpStatusCode.OK, getProjectResponse.StatusCode);
        var projectDetails = await getProjectResponse.Content.ReadFromJsonAsync<JsonElement>();
        var tasks = projectDetails.GetProperty("tasks").EnumerateArray();
        Assert.Single(tasks);

        // Update task
        var updateTaskResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "Updated Task",
            dueDate = "2024-12-31T23:59:59",
            isCompleted = true
        });
        Assert.Equal(HttpStatusCode.OK, updateTaskResponse.StatusCode);

        // Delete task
        var deleteTaskResponse = await _client.DeleteAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteTaskResponse.StatusCode);

        // Delete project
        var deleteProjectResponse = await _client.DeleteAsync($"/api/projects/{projectId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteProjectResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var email = $"test{DateTime.Now.Ticks}@example.com";
        
        // First registration
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "password123"
        });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Duplicate registration
        var duplicateResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "password123"
        });
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nonexistent@example.com",
            password = "wrongpassword"
        });
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Clear();
        
        var response = await _client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
