using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.TimeTracking.Commands.DeleteTimeEntry;
using TaskFlow.Application.TimeTracking.Commands.LogTime;
using TaskFlow.Application.TimeTracking.Dtos;
using TaskFlow.Application.TimeTracking.Queries.GetTaskTimeSummary;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Time tracking endpoints for tasks.</summary>
[ApiController]
[Route("api")]
public sealed class TimeEntriesController(IMediator mediator) : ControllerBase
{
    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>Gets all time entries and the total logged for a task.</summary>
    [HttpGet("tasks/{taskId:guid}/time-entries")]
    [ProducesResponseType(typeof(TaskTimeSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTask(Guid taskId, CancellationToken ct)
        => Ok(await mediator.Send(new GetTaskTimeSummaryQuery(taskId), ct));

    /// <summary>Logs time against a task. The author is the authenticated caller.</summary>
    [HttpPost("tasks/{taskId:guid}/time-entries")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Log(Guid taskId, [FromBody] LogTimeRequest request, CancellationToken ct)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(
            new LogTimeCommand(taskId, userId, request.Minutes, request.Note, request.LoggedAt), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Created(string.Empty, result.Value);
    }

    /// <summary>Deletes a time entry (author only).</summary>
    [HttpDelete("time-entries/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (GetCurrentUserId() is not { } userId)
            return Unauthorized();

        var result = await mediator.Send(new DeleteTimeEntryCommand(id, userId), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == TimeEntryErrors.NotFound.Code) return NotFound(result.Error);
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return NoContent();
    }
}

/// <summary>Payload for logging time against a task.</summary>
public sealed record LogTimeRequest(int Minutes, string? Note, DateTime? LoggedAt);
