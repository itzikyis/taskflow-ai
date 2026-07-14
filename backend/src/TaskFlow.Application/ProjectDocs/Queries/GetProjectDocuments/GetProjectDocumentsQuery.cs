using MediatR;
using TaskFlow.Application.ProjectDocs.Dtos;

namespace TaskFlow.Application.ProjectDocs.Queries.GetProjectDocuments;

/// <summary>Returns all documents for a project.</summary>
public sealed record GetProjectDocumentsQuery(Guid ProjectId) : IRequest<IReadOnlyList<ProjectDocumentDto>>;
