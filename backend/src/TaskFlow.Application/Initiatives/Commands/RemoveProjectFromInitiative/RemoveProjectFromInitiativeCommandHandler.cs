using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.RemoveProjectFromInitiative;

/// <summary>Handles <see cref="RemoveProjectFromInitiativeCommand"/>.</summary>
public sealed class RemoveProjectFromInitiativeCommandHandler(IInitiativeRepository repo)
    : IRequestHandler<RemoveProjectFromInitiativeCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveProjectFromInitiativeCommand request, CancellationToken ct)
    {
        var initiative = await repo.GetByIdAsync(request.InitiativeId, ct);
        if (initiative is null)
            return Result.Failure(new Error("Initiative.NotFound", $"Initiative {request.InitiativeId} not found."));

        var result = initiative.RemoveProject(request.ProjectId);
        if (result.IsFailure) return result;

        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
