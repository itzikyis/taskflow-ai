using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Initiatives.Commands.CreateInitiative;

/// <summary>Handles <see cref="CreateInitiativeCommand"/>.</summary>
public sealed class CreateInitiativeCommandHandler(IInitiativeRepository repo)
    : IRequestHandler<CreateInitiativeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInitiativeCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure(new Error("Initiative.InvalidName", "Name cannot be empty."));

        var initiative = Initiative.Create(
            request.Name, request.Description, request.Priority,
            request.Labels, request.StartDate, request.TargetDate,
            request.CreatedByUserId);

        await repo.AddAsync(initiative, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(initiative.Id);
    }
}
