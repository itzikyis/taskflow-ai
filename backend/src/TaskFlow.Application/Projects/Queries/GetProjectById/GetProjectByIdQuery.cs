using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Queries.GetProjectById;

/// <summary>Query to retrieve a single project by ID.</summary>
public sealed record GetProjectByIdQuery(Guid ProjectId) : IRequest<Result<ProjectDto>>;
