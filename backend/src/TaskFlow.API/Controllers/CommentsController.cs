using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Comments.Commands.AddComment;
using TaskFlow.Application.Comments.Commands.DeleteComment;
using TaskFlow.Application.Comments.Commands.EditComment;
using TaskFlow.Application.Comments.Dtos;
using TaskFlow.Application.Comments.Queries.GetCommentsByTask;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Manages task comments.</summary>
[ApiController]
[Route("api")]
public sealed class CommentsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all comments for a task.</summary>
    [HttpGet("tasks/{taskId:guid}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<CommentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTask(Guid taskId, CancellationToken ct)
        => Ok(await mediator.Send(new GetCommentsByTaskQuery(taskId), ct));

    /// <summary>Adds a comment to a task.</summary>
    [HttpPost("tasks/{taskId:guid}/comments")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add(Guid taskId, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddCommentCommand(taskId, request.AuthorId, request.Content), ct);
        return result.IsFailure ? BadRequest(result.Error) : Created(string.Empty, result.Value);
    }

    /// <summary>Edits a comment.</summary>
    [HttpPut("comments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditCommentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new EditCommentCommand(id, request.RequesterId, request.Content), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == CommentErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>Deletes a comment.</summary>
    [HttpDelete("comments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteCommentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCommentCommand(id, request.RequesterId), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == CommentErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }
}

/// <summary>Payload for adding a comment.</summary>
public sealed record AddCommentRequest(Guid AuthorId, string Content);

/// <summary>Payload for editing a comment.</summary>
public sealed record EditCommentRequest(Guid RequesterId, string Content);

/// <summary>Payload for deleting a comment.</summary>
public sealed record DeleteCommentRequest(Guid RequesterId);
