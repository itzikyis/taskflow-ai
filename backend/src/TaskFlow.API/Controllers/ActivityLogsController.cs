using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Application.ActivityLogs.Queries.GetByActor;
using TaskFlow.Application.ActivityLogs.Queries.GetByEntity;
using TaskFlow.Application.ActivityLogs.Queries.GetByProject;
using TaskFlow.Application.ActivityLogs.Queries.GetRecent;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>Endpoints for querying and recording activity log entries.</summary>
[ApiController]
[Route("api/activity")]
[Authorize]
public sealed class ActivityLogsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Resolves the authenticated caller's user id from the JWT <c>sub</c> claim.
    /// This is the trusted source of actor identity — never trust a client-supplied
    /// id in the request body.
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
    /// <summary>Gets the most recent activity log entries across the system.</summary>
    /// <param name="page">The 1-based page number (default: 1).</param>
    /// <param name="pageSize">The number of results per page (default: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list of activity log entries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetRecentActivityQuery(page, pageSize), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Gets activity log entries for a specific entity.</summary>
    /// <param name="entityType">The entity type (e.g. "Task", "Board").</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="page">The 1-based page number (default: 1).</param>
    /// <param name="pageSize">The number of results per page (default: 30).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list of activity log entries for the entity.</returns>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetActivityByEntityQuery(entityType, entityId, page, pageSize),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Gets activity log entries scoped to a project.</summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="page">The 1-based page number (default: 1).</param>
    /// <param name="pageSize">The number of results per page (default: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list of activity log entries for the project.</returns>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetActivityByProjectQuery(projectId, page, pageSize),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Gets activity log entries performed by a specific actor.</summary>
    /// <param name="actorId">The actor (user) identifier.</param>
    /// <param name="page">The 1-based page number (default: 1).</param>
    /// <param name="pageSize">The number of results per page (default: 30).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list of activity log entries by the actor.</returns>
    [HttpGet("actor/{actorId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByActor(
        Guid actorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetActivityByActorQuery(actorId, page, pageSize),
            cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Records a new activity log entry.</summary>
    /// <param name="request">The activity log payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with the new log entry identifier.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogActivity(
        [FromBody] LogActivityRequest request,
        CancellationToken cancellationToken)
    {
        if (GetCurrentUserId() is not { } actorId)
            return Unauthorized();

        var command = new LogActivityCommand(
            actorId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.EntityName,
            request.ProjectId,
            request.Metadata);

        var result = await mediator.Send(command, cancellationToken);
        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetRecent), new { }, result.Value);
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only — no business logic)
// ---------------------------------------------------------------------------

// Note: the actor identity is intentionally NOT part of this payload — it is
// derived from the authenticated caller's JWT (see GetCurrentUserId) so clients
// cannot forge the actor recorded in the activity log.

/// <summary>Payload for recording an activity log entry.</summary>
public sealed record LogActivityRequest(
    ActivityAction Action,
    string EntityType,
    Guid EntityId,
    string? EntityName,
    Guid? ProjectId,
    string? Metadata);
