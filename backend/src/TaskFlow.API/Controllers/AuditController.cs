using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.AuditTrail.Queries.GetByActor;
using TaskFlow.Application.AuditTrail.Queries.GetByEntity;
using TaskFlow.Application.AuditTrail.Queries.GetRecent;
using TaskFlow.Application.AuditTrail.Dtos;

namespace TaskFlow.API.Controllers;

/// <summary>Read-only endpoints for querying the immutable audit trail.</summary>
[ApiController]
[Route("api/audit")]
public sealed class AuditController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all audit entries for a specific entity, ordered newest-first.</summary>
    /// <param name="entityType">The entity type name (e.g. "Task").</param>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAuditByEntityQuery(entityType, entityId),
            cancellationToken);

        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Returns a paged list of audit entries for a specific actor, ordered newest-first.</summary>
    /// <param name="actorId">The actor's user identifier.</param>
    /// <param name="page">One-based page number (default 1).</param>
    /// <param name="pageSize">Number of entries per page (default 30).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("actor/{actorId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByActor(
        Guid actorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAuditByActorQuery(actorId, page, pageSize),
            cancellationToken);

        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Returns the most recent audit entries across all entities, paged and ordered newest-first.</summary>
    /// <param name="page">One-based page number (default 1).</param>
    /// <param name="pageSize">Number of entries per page (default 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetRecentAuditQuery(page, pageSize),
            cancellationToken);

        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }
}
