using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Commands.DeleteProject;

/// <summary>Handles <see cref="DeleteProjectCommand"/>.</summary>
internal sealed class DeleteProjectCommandHandler(IProjectRepository projectRepository)
    : IRequestHandler<DeleteProjectCommand, Result>
{
    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        projectRepository.Remove(project);
        await projectRepository.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}
