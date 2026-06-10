using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Projects.Queries.GetProjectById;

namespace TaskFlow.Application.Projects.Queries.GetAllProjects;

/// <summary>Handles <see cref="GetAllProjectsQuery"/>.</summary>
internal sealed class GetAllProjectsQueryHandler(IProjectRepository projectRepository)
    : IRequestHandler<GetAllProjectsQuery, IReadOnlyList<ProjectDto>>
{
    public async Task<IReadOnlyList<ProjectDto>> Handle(
        GetAllProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(request.OwnerId, cancellationToken);

        return projects.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.Description,
            p.OwnerId,
            p.CreatedAt,
            p.UpdatedAt)).ToList();
    }
}
