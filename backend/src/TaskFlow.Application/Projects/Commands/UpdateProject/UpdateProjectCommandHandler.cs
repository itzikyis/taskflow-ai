using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Commands.UpdateProject;

/// <summary>Handles <see cref="UpdateProjectCommand"/>.</summary>
internal sealed class UpdateProjectCommandHandler(IProjectRepository projectRepository)
    : IRequestHandler<UpdateProjectCommand, Result>
{
    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        var updateResult = project.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
            return updateResult;

        projectRepository.Update(project);
        await projectRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
