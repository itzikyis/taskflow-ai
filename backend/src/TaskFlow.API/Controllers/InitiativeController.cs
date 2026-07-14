using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Initiatives.Commands.AddProjectToInitiative;
using TaskFlow.Application.Initiatives.Commands.CreateInitiative;
using TaskFlow.Application.Initiatives.Commands.DeleteInitiative;
using TaskFlow.Application.Initiatives.Commands.UpdateInitiativeStatus;
using TaskFlow.Application.Initiatives.Dtos;
using TaskFlow.Application.Initiatives.Queries.GetInitiativeRoadmap;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Controllers;

/// <summary>CRUD and roadmap queries for cross-project initiatives.</summary>
[ApiController]
[Route("api/initiatives")]
public sealed class InitiativeController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all initiatives with aggregate status for the roadmap view.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InitiativeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoadmap(CancellationToken ct)
    {
        var result = await mediator.Send(new GetInitiativeRoadmapQuery(), ct);
        return Ok(result);
    }

    /// <summary>Creates a new initiative.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInitiativeRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<InitiativePriority>(request.Priority, out var priority))
            return BadRequest($"Unknown priority '{request.Priority}'.");

        var result = await mediator.Send(new CreateInitiativeCommand(
            request.Name, request.Description ?? string.Empty,
            priority, request.Labels ?? string.Empty,
            request.StartDate, request.TargetDate,
            request.CreatedByUserId), ct);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetRoadmap), result.Value);
    }

    /// <summary>Updates the status of an initiative.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<InitiativeStatus>(request.Status, out var status))
            return BadRequest($"Unknown status '{request.Status}'.");

        var result = await mediator.Send(new UpdateInitiativeStatusCommand(id, status), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>Links a project to an initiative.</summary>
    [HttpPost("{id:guid}/projects/{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProject(Guid id, Guid projectId, CancellationToken ct)
    {
        var result = await mediator.Send(new AddProjectToInitiativeCommand(id, projectId), ct);
        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Deletes an initiative.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteInitiativeCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

/// <summary>Payload for creating an initiative.</summary>
public sealed record CreateInitiativeRequest(
    string Name,
    string? Description,
    string Priority,
    string? Labels,
    DateTime? StartDate,
    DateTime? TargetDate,
    Guid CreatedByUserId);

/// <summary>Payload for updating initiative status.</summary>
public sealed record UpdateStatusRequest(string Status);
