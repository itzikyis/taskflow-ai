using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Goals.Commands.CreateGoal;
using TaskFlow.Application.Goals.Commands.DeleteGoal;
using TaskFlow.Application.Goals.Commands.UpdateGoalProgress;
using TaskFlow.Application.Goals.Queries.GetGoalsByProject;

namespace TaskFlow.API.Controllers;

/// <summary>Endpoints for OKR/Goals management.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class GoalsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all goals for the specified project.</summary>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<GoalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetGoalsByProjectQuery(projectId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new Goal (Objective).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateGoalCommand(
                request.ProjectId,
                request.OwnerId,
                request.Title,
                request.Description,
                request.DueDate),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetByProject), new { projectId = request.ProjectId }, result.Value);
    }

    /// <summary>Updates the progress percentage and status of a Goal.</summary>
    [HttpPut("{id:guid}/progress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProgress(
        Guid id,
        [FromBody] UpdateGoalProgressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateGoalProgressCommand(id, request.ProgressPercent, request.Status),
            cancellationToken);

        if (result.IsFailure && result.Error.Code == "Goal.NotFound")
            return NotFound(result.Error);

        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>Deletes a Goal permanently.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteGoalCommand(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only)
// ---------------------------------------------------------------------------

/// <summary>Payload for creating a goal.</summary>
public sealed record CreateGoalRequest(
    Guid ProjectId,
    Guid OwnerId,
    string Title,
    string? Description,
    DateTime? DueDate);

/// <summary>Payload for updating a goal's progress and status.</summary>
public sealed record UpdateGoalProgressRequest(int ProgressPercent, string Status);
