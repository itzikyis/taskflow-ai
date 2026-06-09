using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Tasks.Commands.CreateTask;
using TaskFlow.Application.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Tasks.Commands.UpdateTask;
using TaskFlow.Application.Tasks.Queries.GetAllTasks;
using TaskFlow.Application.Tasks.Queries.GetTaskById;
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
            new UpdateTaskCommand(id, request.Title, request.Description),
            cancellationToken);

        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Deletes a task permanently.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTaskCommand(id), cancellationToken);
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
public sealed record UpdateTaskRequest(string Title, string? Description);
