using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.AddProjectToInitiative;

/// <summary>Handles <see cref="AddProjectToInitiativeCommand"/>.</summary>
public sealed class AddProjectToInitiativeCommandHandler(IInitiativeRepository repo)
    : IRequestHandler<AddProjectToInitiativeCommand, Result>
{
    public async Task<Result> Handle(AddProjectToInitiativeCommand request, CancellationToken ct)
    {
        var initiative = await repo.GetByIdAsync(request.InitiativeId, ct);
        if (initiative is null)
            return Result.Failure(new Error("Initiative.NotFound", $"Initiative {request.InitiativeId} not found."));

        var result = initiative.AddProject(request.ProjectId);
        if (result.IsFailure) return result;

        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
