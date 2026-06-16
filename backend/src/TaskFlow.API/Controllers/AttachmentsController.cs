using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Attachments.Commands.AddAttachment;
using TaskFlow.Application.Attachments.Commands.DeleteAttachment;
using TaskFlow.Application.Attachments.Dtos;
using TaskFlow.Application.Attachments.Queries.GetAttachmentsByTask;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Manages task file attachments.</summary>
[ApiController]
[Route("api")]
public sealed class AttachmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets all attachments for a task.</summary>
    [HttpGet("tasks/{taskId:guid}/attachments")]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTask(Guid taskId, CancellationToken ct)
        => Ok(await mediator.Send(new GetAttachmentsByTaskQuery(taskId), ct));

    /// <summary>Records a new attachment on a task.</summary>
    [HttpPost("tasks/{taskId:guid}/attachments")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add(Guid taskId, [FromBody] AddAttachmentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddAttachmentCommand(taskId, request.UploadedBy, request.FileName, request.ContentType, request.FileSizeBytes, request.StorageUrl), ct);
        return result.IsFailure ? BadRequest(result.Error) : Created(string.Empty, result.Value);
    }

    /// <summary>Deletes an attachment.</summary>
    [HttpDelete("attachments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteAttachmentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAttachmentCommand(id, request.RequesterId), ct);
        if (result.IsFailure)
        {
            if (result.Error.Code == AttachmentErrors.NotFound.Code) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }
}

/// <summary>Payload for recording a new attachment.</summary>
public sealed record AddAttachmentRequest(
    Guid UploadedBy,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StorageUrl);

/// <summary>Payload for deleting an attachment.</summary>
public sealed record DeleteAttachmentRequest(Guid RequesterId);
