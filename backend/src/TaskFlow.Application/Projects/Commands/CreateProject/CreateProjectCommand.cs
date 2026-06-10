using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

/// <summary>Command to create a new project.</summary>
public sealed record CreateProjectCommand(
    string Name,
    string? Description,
    Guid OwnerId) : IRequest<Result<Guid>>;
