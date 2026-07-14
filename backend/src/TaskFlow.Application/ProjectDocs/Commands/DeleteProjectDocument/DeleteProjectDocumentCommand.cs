using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ProjectDocs.Commands.DeleteProjectDocument;

/// <summary>Permanently removes a project document.</summary>
public sealed record DeleteProjectDocumentCommand(Guid Id) : IRequest<Result>;
