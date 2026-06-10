using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Commands.UpdateProject;

/// <summary>Command to update a project's name and description.</summary>
public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Name,
    string? Description) : IRequest<Result>;
