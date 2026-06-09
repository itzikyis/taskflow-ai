using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for the Tasks API endpoints.
/// Requires a running PostgreSQL instance (use Testcontainers in CI).
/// </summary>
public sealed class TasksEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTask_ValidPayload_ReturnsCreated()
    {
        var payload = new
        {
            title = "Integration test task",
            description = (string?)null,
            priority = TaskPriority.Low,
            createdByUserId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTask_EmptyTitle_ReturnsBadRequest()
    {
        var payload = new
        {
            title = "",
            priority = TaskPriority.Low,
            createdByUserId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
