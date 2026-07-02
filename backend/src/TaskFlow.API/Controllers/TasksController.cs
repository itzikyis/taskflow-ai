using MediatR;
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
    /// <summary>Gets all tasks, optionally filtered by assigned user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? assignedToUserId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTasksQuery(assignedToUserId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a task by its unique identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTaskByIdQuery(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new task.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.Priority,
            request.CreatedByUserId);

        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>Updates a task's title and description.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTaskCommand(id, request.Title, request.Description, request.ActorId),
            cancellationToken);

        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Transitions a task to a new status.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTaskStatusCommand(id, request.Status, request.ActorId),
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToColumn(
        Guid id,
        [FromBody] MoveTaskToColumnRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MoveTaskToColumnCommand(id, request.ColumnId, request.ActorId), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>Deletes a task permanently.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromBody] DeleteTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTaskCommand(id, request.ActorId), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only — no business logic)
// ---------------------------------------------------------------------------

/// <summary>Payload for creating a task.</summary>
public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    Guid CreatedByUserId);

/// <summary>Payload for updating a task.</summary>
public sealed record UpdateTaskRequest(string Title, string? Description, Guid ActorId);

/// <summary>Payload for updating a task's status.</summary>
public sealed record UpdateTaskStatusRequest(TaskItemStatus Status, Guid ActorId);

/// <summary>Payload for moving a task to a board column.</summary>
public sealed record MoveTaskToColumnRequest(Guid? ColumnId, Guid ActorId);

/// <summary>Payload for deleting a task.</summary>
public sealed record DeleteTaskRequest(Guid ActorId);
