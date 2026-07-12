using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Dependencies.Commands.AddDependency;
using TaskFlow.Application.Dependencies.Commands.RemoveDependency;
using TaskFlow.Application.Dependencies.Dtos;
using TaskFlow.Application.Dependencies.Queries.GetAllDependencies;
using TaskFlow.Application.Dependencies.Queries.GetTaskDependencies;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Manages finish-to-start dependencies between tasks.</summary>
[ApiController]
[Route("api")]
public sealed class DependenciesController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets the blockers and blocked tasks for a task.</summary>
    [HttpGet("tasks/{taskId:guid}/dependencies")]
    [ProducesResponseType(typeof(TaskDependenciesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForTask(Guid taskId, CancellationToken ct)
        => Ok(await mediator.Send(new GetTaskDependenciesQuery(taskId), ct));

    /// <summary>Gets every dependency edge (for the timeline view).</summary>
    [HttpGet("dependencies")]
    [ProducesResponseType(typeof(IReadOnlyList<DependencyEdgeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetAllDependenciesQuery(), ct));

    /// <summary>Declares that a task is blocked by another task.</summary>
    [HttpPost("tasks/{taskId:guid}/dependencies")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(Guid taskId, [FromBody] AddDependencyRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddDependencyCommand(taskId, request.BlockedByTaskId), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return Created(string.Empty, result.Value);
    }

    /// <summary>Removes a dependency.</summary>
    [HttpDelete("dependencies/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveDependencyCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

/// <summary>Payload for adding a dependency.</summary>
public sealed record AddDependencyRequest(Guid BlockedByTaskId);
