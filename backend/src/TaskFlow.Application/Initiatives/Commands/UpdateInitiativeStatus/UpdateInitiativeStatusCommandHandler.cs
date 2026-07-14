using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Initiatives.Commands.UpdateInitiativeStatus;

/// <summary>Handles <see cref="UpdateInitiativeStatusCommand"/>.</summary>
public sealed class UpdateInitiativeStatusCommandHandler(IInitiativeRepository repo)
    : IRequestHandler<UpdateInitiativeStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateInitiativeStatusCommand request, CancellationToken ct)
    {
        var initiative = await repo.GetByIdAsync(request.Id, ct);
        if (initiative is null)
            return Result.Failure(new Error("Initiative.NotFound", $"Initiative {request.Id} not found."));

        initiative.UpdateStatus(request.Status);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
