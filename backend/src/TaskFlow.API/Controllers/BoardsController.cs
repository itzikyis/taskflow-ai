using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Boards.Commands.AddColumn;
using TaskFlow.Application.Boards.Commands.CreateBoard;
using TaskFlow.Application.Boards.Commands.DeleteBoard;
using TaskFlow.Application.Boards.Commands.RemoveColumn;
using TaskFlow.Application.Boards.Commands.UpdateBoard;
using TaskFlow.Application.Boards.Dtos;
using TaskFlow.Application.Boards.Queries.GetBoardById;
using TaskFlow.Application.Boards.Queries.GetBoardsByProject;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Manages Kanban boards and their columns.</summary>
[ApiController]
[Route("api")]
public sealed class BoardsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all boards for a project.</summary>
    [HttpGet("projects/{projectId:guid}/boards")]
    [ProducesResponseType(typeof(IReadOnlyList<BoardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
        => Ok(await mediator.Send(new GetBoardsByProjectQuery(projectId), ct));

    /// <summary>Gets a board by id including its columns.</summary>
    [HttpGet("boards/{id:guid}")]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBoardByIdQuery(id), ct);
        return result.IsFailure ? NotFound(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new board for a project.</summary>
    [HttpPost("boards")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateBoardCommand(request.Name, request.ProjectId), ct);
        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>Updates a board's name.</summary>
    [HttpPut("boards/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoardRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateBoardCommand(id, request.Name), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == BoardErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>Deletes a board.</summary>
    [HttpDelete("boards/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteBoardCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>Adds a column to a board.</summary>
    [HttpPost("boards/{id:guid}/columns")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddColumn(Guid id, [FromBody] AddColumnRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddColumnCommand(id, request.Name, request.Order, request.WipLimit), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == BoardErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return Created(string.Empty, result.Value);
    }

    /// <summary>Removes a column from a board.</summary>
    [HttpDelete("boards/{id:guid}/columns/{columnId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveColumn(Guid id, Guid columnId, CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveColumnCommand(id, columnId), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

/// <summary>Payload for creating a board.</summary>
public sealed record CreateBoardRequest(string Name, Guid ProjectId);

/// <summary>Payload for updating a board.</summary>
public sealed record UpdateBoardRequest(string Name);

/// <summary>Payload for adding a column.</summary>
public sealed record AddColumnRequest(string Name, int Order, int? WipLimit);
