using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Commands.DeleteProject;

/// <summary>Command to permanently delete a project.</summary>
public sealed record DeleteProjectCommand(Guid ProjectId) : IRequest<Result>;
