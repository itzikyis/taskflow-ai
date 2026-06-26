using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Teams.Commands.AddMember;
using TaskFlow.Application.Teams.Commands.CreateTeam;
using TaskFlow.Application.Teams.Commands.DeleteTeam;
using TaskFlow.Application.Teams.Commands.RemoveMember;
using TaskFlow.Application.Teams.Commands.UpdateMemberRole;
using TaskFlow.Application.Teams.Queries.GetAllTeams;
using TaskFlow.Application.Teams.Queries.GetTeamById;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>Endpoints for team management.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class TeamsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all teams including their members.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTeamsQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Gets a team by its unique identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeamByIdQuery(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new team.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTeamRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTeamCommand(request.Name, request.Description),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>Deletes a team by its unique identifier.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTeamCommand(id), cancellationToken);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>Adds a member to the specified team.</summary>
    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return BadRequest(new { error = "UserId must be a valid UUID." });

        var result = await mediator.Send(
            new AddMemberCommand(id, userId, request.Role),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == TeamErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>Removes a member from the specified team.</summary>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveMemberCommand(id, userId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == TeamErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>Updates the role of a team member.</summary>
    [HttpPut("{id:guid}/members/{userId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberRole(
        Guid id,
        Guid userId,
        [FromBody] UpdateMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateMemberRoleCommand(id, userId, request.Role),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == TeamErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}

// ---------------------------------------------------------------------------
// Request records (API layer only — no business logic)
// ---------------------------------------------------------------------------

/// <summary>Payload for creating a team.</summary>
public sealed record CreateTeamRequest(string Name, string? Description);

/// <summary>Payload for adding a team member.</summary>
public sealed record AddMemberRequest(string UserId, TeamRole Role);

/// <summary>Payload for updating a team member's role.</summary>
public sealed record UpdateMemberRoleRequest(TeamRole Role);
