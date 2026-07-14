using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.ProjectDocs.Commands.CreateProjectDocument;
using TaskFlow.Application.ProjectDocs.Commands.DeleteProjectDocument;
using TaskFlow.Application.ProjectDocs.Commands.UpdateProjectDocument;
using TaskFlow.Application.ProjectDocs.Dtos;
using TaskFlow.Application.ProjectDocs.Queries.GetProjectDocuments;

namespace TaskFlow.API.Controllers;

/// <summary>CRUD for project documents (wiki/specs space).</summary>
[ApiController]
[Route("api/project-docs")]
public sealed class ProjectDocumentController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all documents for a project.</summary>
    [HttpGet("projects/{projectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
    {
        var docs = await mediator.Send(new GetProjectDocumentsQuery(projectId), ct);
        return Ok(docs);
    }

    /// <summary>Creates a new document for a project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDocRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateProjectDocumentCommand(
            request.ProjectId, request.Title, request.Body ?? string.Empty, request.AuthorId), ct);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetByProject), new { projectId = request.ProjectId }, result.Value);
    }

    /// <summary>Updates a document's title and body.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateProjectDocumentCommand(id, request.Title, request.Body ?? string.Empty), ct);
        return result.IsFailure ? (result.Error.Code.Contains("NotFound") ? NotFound(result.Error) : BadRequest(result.Error)) : NoContent();
    }

    /// <summary>Deletes a document.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteProjectDocumentCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

/// <summary>Payload for creating a document.</summary>
public sealed record CreateDocRequest(Guid ProjectId, string Title, string? Body, Guid AuthorId);

/// <summary>Payload for updating a document.</summary>
public sealed record UpdateDocRequest(string Title, string? Body);
