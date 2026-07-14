using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ProjectDocs.Commands.UpdateProjectDocument;

/// <summary>Updates the title and body of an existing project document.</summary>
public sealed record UpdateProjectDocumentCommand(Guid Id, string Title, string Body) : IRequest<Result>;
