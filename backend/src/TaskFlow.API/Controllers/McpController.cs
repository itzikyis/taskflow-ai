using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects.Queries.GetAllProjects;
using TaskFlow.Application.Search.Queries.SearchTasks;
using TaskFlow.Application.Tasks.Commands.CreateTask;
using TaskFlow.Application.Tasks.Queries.GetAllTasks;
using TaskFlow.Application.Tasks.Queries.GetTaskById;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>
/// MCP (Model Context Protocol) endpoint — exposes TaskFlow AI tools to external
/// AI assistants such as Claude Desktop and GitHub Copilot.
/// </summary>
[ApiController]
[Route("api/mcp")]
public sealed class McpController(IMediator mediator) : ControllerBase
{
    // -----------------------------------------------------------------------
    // Tool manifest (static — no auth required)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the MCP tool manifest describing all available tools and their
    /// JSON-Schema input definitions.  No authentication is required; this is a
    /// public capability advertisement.
    /// </summary>
    [HttpGet("tools")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(McpToolsResponse), StatusCodes.Status200OK)]
    public IActionResult GetTools()
    {
        var tools = new McpToolsResponse(
        [
            new McpTool(
                "list_tasks",
                "List all tasks in TaskFlow AI",
                new McpInputSchema("object", new Dictionary<string, McpProperty>(), [])),

            new McpTool(
                "create_task",
                "Create a new task in TaskFlow AI",
                new McpInputSchema(
                    "object",
                    new Dictionary<string, McpProperty>
                    {
                        ["title"]       = new("string",  "Title of the task (required)"),
                        ["description"] = new("string",  "Optional longer description"),
                        ["priority"]    = new("string",  "Priority level: Low, Medium, High, or Critical"),
                        ["dueDate"]     = new("string",  "ISO-8601 date, e.g. 2025-12-31 (optional)")
                    },
                    ["title"])),

            new McpTool(
                "get_task",
                "Get a single task by its unique ID",
                new McpInputSchema(
                    "object",
                    new Dictionary<string, McpProperty>
                    {
                        ["id"] = new("string", "Task GUID, e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6")
                    },
                    ["id"])),

            new McpTool(
                "list_projects",
                "List all projects in TaskFlow AI",
                new McpInputSchema("object", new Dictionary<string, McpProperty>(), [])),

            new McpTool(
                "search_tasks",
                "Search tasks using a natural-language query",
                new McpInputSchema(
                    "object",
                    new Dictionary<string, McpProperty>
                    {
                        ["query"] = new("string", "Free-text search, e.g. \"overdue high-priority tasks\"")
                    },
                    ["query"]))
        ]);

        return Ok(tools);
    }

    // -----------------------------------------------------------------------
    // Tool call dispatcher (requires auth)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Dispatches an MCP tool call to the appropriate MediatR handler and returns
    /// the result wrapped in <c>{ "result": … }</c>.  A valid Bearer token must be
    /// supplied in the <c>Authorization</c> header.
    /// </summary>
    /// <remarks>
    /// Request body: <c>{ "tool": "&lt;toolName&gt;", "parameters": { … } }</c>
    /// </remarks>
    [HttpPost("call")]
    [Authorize]
    [ProducesResponseType(typeof(McpCallResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Call(
        [FromBody] McpCallRequest request,
        CancellationToken cancellationToken)
    {
        var parameters = request.Parameters ?? new Dictionary<string, JsonElement>();

        switch (request.Tool)
        {
            case "list_tasks":
                return await HandleListTasks(parameters, cancellationToken);

            case "create_task":
                return await HandleCreateTask(parameters, cancellationToken);

            case "get_task":
                return await HandleGetTask(parameters, cancellationToken);

            case "list_projects":
                return await HandleListProjects(parameters, cancellationToken);

            case "search_tasks":
                return await HandleSearchTasks(parameters, cancellationToken);

            default:
                return BadRequest(new McpErrorResponse($"Unknown tool: '{request.Tool}'"));
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers — one per tool
    // -----------------------------------------------------------------------

    private async Task<IActionResult> HandleListTasks(
        Dictionary<string, JsonElement> _,
        CancellationToken ct)
    {
        var tasks = await mediator.Send(new GetAllTasksQuery(), ct);
        return Ok(new McpCallResponse(tasks));
    }

    private async Task<IActionResult> HandleCreateTask(
        Dictionary<string, JsonElement> parameters,
        CancellationToken ct)
    {
        if (!TryGetString(parameters, "title", out var title))
            return BadRequest(new McpErrorResponse("Parameter 'title' is required for create_task"));

        TryGetString(parameters, "description", out var description);
        TryGetString(parameters, "dueDate",     out var dueDateStr);
        TryGetString(parameters, "priority",    out var priorityStr);

        var priority = Enum.TryParse<TaskPriority>(priorityStr, ignoreCase: true, out var p)
            ? p
            : TaskPriority.Medium;

        DateTime? dueDate = DateTime.TryParse(dueDateStr, out var d) ? d : null;

        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var command = new CreateTaskCommand(title!, description, priority, userId, dueDate);
        var result  = await mediator.Send(command, ct);

        return result.IsFailure
            ? BadRequest(new McpErrorResponse(result.Error.Description))
            : Ok(new McpCallResponse(result.Value));
    }

    private async Task<IActionResult> HandleGetTask(
        Dictionary<string, JsonElement> parameters,
        CancellationToken ct)
    {
        if (!TryGetString(parameters, "id", out var idStr) || !Guid.TryParse(idStr, out var id))
            return BadRequest(new McpErrorResponse("Parameter 'id' must be a valid GUID for get_task"));

        var result = await mediator.Send(new GetTaskByIdQuery(id), ct);
        return result.IsFailure
            ? NotFound(new McpErrorResponse(result.Error.Description))
            : Ok(new McpCallResponse(result.Value));
    }

    private async Task<IActionResult> HandleListProjects(
        Dictionary<string, JsonElement> _,
        CancellationToken ct)
    {
        var projects = await mediator.Send(new GetAllProjectsQuery(), ct);
        return Ok(new McpCallResponse(projects));
    }

    private async Task<IActionResult> HandleSearchTasks(
        Dictionary<string, JsonElement> parameters,
        CancellationToken ct)
    {
        if (!TryGetString(parameters, "query", out var query))
            return BadRequest(new McpErrorResponse("Parameter 'query' is required for search_tasks"));

        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(new SearchTasksQuery(query!, userId), ct);
        return Ok(new McpCallResponse(result));
    }

    // -----------------------------------------------------------------------
    // Utility
    // -----------------------------------------------------------------------

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static bool TryGetString(
        Dictionary<string, JsonElement> parameters,
        string key,
        out string? value)
    {
        if (parameters.TryGetValue(key, out var element) &&
            element.ValueKind == JsonValueKind.String)
        {
            value = element.GetString();
            return value is not null;
        }

        value = null;
        return false;
    }
}

// ---------------------------------------------------------------------------
// Request / response DTOs (API layer only)
// ---------------------------------------------------------------------------

/// <summary>Inbound MCP tool-call payload.</summary>
public sealed record McpCallRequest(
    string Tool,
    Dictionary<string, JsonElement>? Parameters);

/// <summary>Successful MCP tool-call response.</summary>
public sealed record McpCallResponse(object? Result);

/// <summary>MCP error response returned when a tool call fails.</summary>
public sealed record McpErrorResponse(string Error);

/// <summary>MCP tools manifest returned by GET /api/mcp/tools.</summary>
public sealed record McpToolsResponse(IReadOnlyList<McpTool> Tools);

/// <summary>Describes a single MCP tool.</summary>
public sealed record McpTool(
    string Name,
    string Description,
    McpInputSchema InputSchema);

/// <summary>JSON-Schema descriptor for a tool's input parameters.</summary>
public sealed record McpInputSchema(
    string Type,
    Dictionary<string, McpProperty> Properties,
    IReadOnlyList<string> Required);

/// <summary>A single property in a tool's JSON-Schema input.</summary>
public sealed record McpProperty(string Type, string Description);
