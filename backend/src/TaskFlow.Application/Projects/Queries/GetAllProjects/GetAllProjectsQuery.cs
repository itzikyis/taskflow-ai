using MediatR;
using TaskFlow.Application.Projects.Queries.GetProjectById;

namespace TaskFlow.Application.Projects.Queries.GetAllProjects;

/// <summary>Query to list all projects, optionally filtered by owner.</summary>
public sealed record GetAllProjectsQuery(Guid? OwnerId = null)
    : IRequest<IReadOnlyList<ProjectDto>>;
