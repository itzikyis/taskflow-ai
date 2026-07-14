using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.DeleteInitiative;

/// <summary>Handles <see cref="DeleteInitiativeCommand"/>.</summary>
public sealed class DeleteInitiativeCommandHandler(IInitiativeRepository repo)
    : IRequestHandler<DeleteInitiativeCommand, Result>
{
    public async Task<Result> Handle(DeleteInitiativeCommand request, CancellationToken ct)
    {
        var initiative = await repo.GetByIdAsync(request.Id, ct);
        if (initiative is null)
            return Result.Failure(new Error("Initiative.NotFound", $"Initiative {request.Id} not found."));

        await repo.DeleteAsync(initiative, ct);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
