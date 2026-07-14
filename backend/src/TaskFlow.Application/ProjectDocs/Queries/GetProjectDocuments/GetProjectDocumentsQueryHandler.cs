using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.ProjectDocs.Dtos;

namespace TaskFlow.Application.ProjectDocs.Queries.GetProjectDocuments;

/// <summary>Handles <see cref="GetProjectDocumentsQuery"/>.</summary>
public sealed class GetProjectDocumentsQueryHandler(IProjectDocumentRepository repo)
    : IRequestHandler<GetProjectDocumentsQuery, IReadOnlyList<ProjectDocumentDto>>
{
    public async Task<IReadOnlyList<ProjectDocumentDto>> Handle(
        GetProjectDocumentsQuery request, CancellationToken ct)
    {
        var docs = await repo.GetByProjectAsync(request.ProjectId, ct);
        return docs.Select(d => new ProjectDocumentDto(
            d.Id, d.ProjectId, d.Title, d.Body, d.AuthorId, d.CreatedAt, d.UpdatedAt))
            .ToList();
    }
}
