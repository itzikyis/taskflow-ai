using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DevelopmentLinks.Commands.IngestGitHubEvent;
using TaskFlow.Application.DevelopmentLinks.Commands.LinkDevelopment;
using TaskFlow.Application.DevelopmentLinks.Commands.RemoveDevelopmentLink;
using TaskFlow.Application.DevelopmentLinks.Dtos;
using TaskFlow.Application.DevelopmentLinks.Queries.GetByTask;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>Links tasks to source-control references (branches, commits, pull requests).</summary>
[ApiController]
[Route("api")]
public sealed class DevelopmentController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all development links for a task.</summary>
    [HttpGet("tasks/{taskId:guid}/development")]
    [ProducesResponseType(typeof(IReadOnlyList<DevelopmentLinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTask(Guid taskId, CancellationToken ct)
        => Ok(await mediator.Send(new GetDevelopmentLinksByTaskQuery(taskId), ct));

    /// <summary>Manually links a source-control reference to a task.</summary>
    [HttpPost("tasks/{taskId:guid}/development")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Link(
        Guid taskId,
        [FromBody] LinkDevelopmentRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new LinkDevelopmentCommand(
                taskId, request.Repository, request.RefType,
                request.Title, request.Url, request.Status, request.ExternalId),
            ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == TaskErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Created(string.Empty, result.Value);
    }

    /// <summary>Removes a development link.</summary>
    [HttpDelete("development/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveDevelopmentLinkCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>
    /// Receives a GitHub webhook (push / pull_request), detects task references in
    /// commit messages, branch names and PR titles, and links them to tasks.
    /// </summary>
    [HttpPost("integrations/github/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GitHubWebhook(CancellationToken ct)
    {
        var eventType = Request.Headers["X-GitHub-Event"].ToString();
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        var result = await mediator.Send(new IngestGitHubEventCommand(eventType, payload), ct);
        return result.IsFailure
            ? BadRequest(result.Error)
            : Ok(new { linked = result.Value });
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (API layer only)
// ---------------------------------------------------------------------------

/// <summary>Payload for manually linking a development reference to a task.</summary>
public sealed record LinkDevelopmentRequest(
    string Repository,
    DevelopmentRefType RefType,
    string Title,
    string Url,
    DevelopmentLinkStatus Status = DevelopmentLinkStatus.None,
    string? ExternalId = null);
