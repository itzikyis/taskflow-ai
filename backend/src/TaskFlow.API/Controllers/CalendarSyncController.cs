using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.CalendarSync.Commands.AddCalendarSubscription;
using TaskFlow.Application.CalendarSync.Commands.RemoveCalendarSubscription;
using TaskFlow.Application.CalendarSync.Commands.SyncCalendarSubscription;
using TaskFlow.Application.CalendarSync.Dtos;
using TaskFlow.Application.CalendarSync.Queries.GetCalendarSubscriptions;

namespace TaskFlow.API.Controllers;

/// <summary>Manages two-way calendar sync subscriptions (external iCal feeds → task due dates).</summary>
[ApiController]
[Route("api/calendar/subscriptions")]
[Authorize]
public sealed class CalendarSyncController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all calendar subscriptions for the specified project.</summary>
    /// <param name="projectId">The project whose subscriptions to list.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(List<CalendarSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubscriptions(Guid projectId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCalendarSubscriptionsQuery(projectId), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new external iCal subscription for a project.</summary>
    /// <param name="command">The subscription details.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddSubscription(
        [FromBody] AddCalendarSubscriptionCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetSubscriptions),
            new { projectId = command.ProjectId },
            new { id = result.Value });
    }

    /// <summary>Removes a calendar subscription.</summary>
    /// <param name="id">The ID of the subscription to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveSubscription(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveCalendarSubscriptionCommand(id), ct);
        return result.IsFailure ? BadRequest(result.Error) : NoContent();
    }

    /// <summary>
    /// Triggers an immediate sync of the specified subscription.
    /// Returns the number of tasks created or updated.
    /// </summary>
    /// <param name="id">The ID of the subscription to sync.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{id:guid}/sync")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SyncSubscription(Guid id, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new SyncCalendarSubscriptionCommand(id, userId), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(new { tasksAffected = result.Value });
    }
}
