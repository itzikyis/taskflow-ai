using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.TaskTemplates.Commands.CreateTaskFromTemplate;
using TaskFlow.Application.TaskTemplates.Commands.CreateTaskTemplate;
using TaskFlow.Application.TaskTemplates.Commands.DeleteTaskTemplate;
using TaskFlow.Application.TaskTemplates.Dtos;
using TaskFlow.Application.TaskTemplates.Queries.GetTaskTemplates;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Endpoints for managing and using task templates.</summary>
[ApiController]
[Route("api/task-templates")]
[Authorize]
public sealed class TaskTemplatesController(IMediator mediator) : ControllerBase
{
    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>Returns all task templates for a given project.</summary>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TaskTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTaskTemplatesQuery(projectId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new task template.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskTemplateCommand(
            request.ProjectId,
            request.Name,
            request.DefaultTitle,
            request.DefaultDescription,
            request.DefaultPriority,
            request.DefaultEstimatedHours);

        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetByProject), new { projectId = request.ProjectId }, result.Value);
    }

    /// <summary>Deletes a task template by id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTaskTemplateCommand(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>Instantiates a new task from the given template and returns the new task id.</summary>
    [HttpPost("{id:guid}/create-task")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTask(Guid id, CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(new CreateTaskFromTemplateCommand(id, userId), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error.Code == TaskTemplateErrors.NotFound.Code)
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Created(string.Empty, result.Value);
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only)
// ---------------------------------------------------------------------------

/// <summary>Payload for creating a task template.</summary>
public sealed record CreateTaskTemplateRequest(
    Guid ProjectId,
    string Name,
    string DefaultTitle,
    string? DefaultDescription,
    string? DefaultPriority,
    int? DefaultEstimatedHours);
