using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks.Commands.CreateTask;
using TaskFlow.Application.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Tasks.Commands.UpdateTask;
using TaskFlow.Application.Tasks.Commands.MoveTaskToColumn;
using TaskFlow.Application.Tasks.Commands.UpdateTaskStatus;
using TaskFlow.Application.Tasks.Queries.GetAllTasks;
using TaskFlow.Application.Tasks.Queries.GetTaskById;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>CRUD endpoints for task management.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TasksController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Resolves the authenticated caller's user id from the JWT <c>sub</c> claim.
    /// This is the trusted source of actor identity for auditing — never trust a
    /// client-supplied id in the request body.
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
    /// <summary>Gets all tasks, optionally filtered by assigned user.</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? assignedToUserId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTasksQuery(assignedToUserId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a task by its unique identifier.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTaskByIdQuery(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new task. The creator is the authenticated caller.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.Priority,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>Updates a task's title and description.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(
            new UpdateTaskCommand(id, request.Title, request.Description, userId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>Transitions a task to a new status.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(
            new UpdateTaskStatusCommand(id, request.Status, userId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>Moves a task into a board column (pass null columnId to remove from board).</summary>
    [HttpPatch("{id:guid}/column")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToColumn(
        Guid id,
        [FromBody] MoveTaskToColumnRequest request,
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(new MoveTaskToColumnCommand(id, request.ColumnId, userId), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>Deletes a task permanently.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(new DeleteTaskCommand(id, userId), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only — no business logic)
// ---------------------------------------------------------------------------

// Note: actor/creator identity is intentionally NOT part of these payloads —
// it is derived from the authenticated caller's JWT (see GetCurrentUserId) so
// clients cannot forge the identity recorded in the audit trail / activity log.

/// <summary>Payload for creating a task.</summary>
public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority);

/// <summary>Payload for updating a task.</summary>
public sealed record UpdateTaskRequest(string Title, string? Description);

/// <summary>Payload for updating a task's status.</summary>
public sealed record UpdateTaskStatusRequest(TaskItemStatus Status);

/// <summary>Payload for moving a task to a board column.</summary>
public sealed record MoveTaskToColumnRequest(Guid? ColumnId);
