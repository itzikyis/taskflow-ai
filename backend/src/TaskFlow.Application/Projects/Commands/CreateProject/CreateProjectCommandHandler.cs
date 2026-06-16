using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

/// <summary>Handles <see cref="CreateProjectCommand"/>.</summary>
public sealed class CreateProjectCommandHandler(IProjectRepository projectRepository)
    : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var result = Project.Create(request.Name, request.Description, request.OwnerId);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await projectRepository.AddAsync(result.Value!, cancellationToken);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
