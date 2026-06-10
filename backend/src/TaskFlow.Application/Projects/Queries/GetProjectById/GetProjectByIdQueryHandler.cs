using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Queries.GetProjectById;

/// <summary>Handles <see cref="GetProjectByIdQuery"/>.</summary>
internal sealed class GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            return Result<ProjectDto>.Failure(ProjectErrors.NotFound);

        return Result<ProjectDto>.Success(new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.CreatedAt,
            project.UpdatedAt));
    }
}
