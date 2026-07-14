using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ProjectDocs.Commands.CreateProjectDocument;

/// <summary>Creates a new project document.</summary>
public sealed record CreateProjectDocumentCommand(
    Guid ProjectId,
    string Title,
    string Body,
    Guid AuthorId) : IRequest<Result<Guid>>;
