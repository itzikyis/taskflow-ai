using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects.Commands.CreateProject;
using TaskFlow.Application.Projects.Commands.DeleteProject;
using TaskFlow.Application.Projects.Commands.UpdateProject;
using TaskFlow.Application.Projects.Queries.GetAllProjects;
using TaskFlow.Application.Projects.Queries.GetProjectById;

namespace TaskFlow.API.Controllers;

/// <summary>CRUD endpoints for project management.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ProjectsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all projects, optionally filtered by owner.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? ownerId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllProjectsQuery(ownerId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a project by its unique identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateProjectCommand(request.Name, request.Description, request.OwnerId),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>Updates a project's name and description.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateProjectCommand(id, request.Name, request.Description),
            cancellationToken);

        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Deletes a project permanently.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteProjectCommand(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only)
// ---------------------------------------------------------------------------

/// <summary>Payload for creating a project.</summary>
public sealed record CreateProjectRequest(string Name, string? Description, Guid OwnerId);

/// <summary>Payload for updating a project.</summary>
public sealed record UpdateProjectRequest(string Name, string? Description);
