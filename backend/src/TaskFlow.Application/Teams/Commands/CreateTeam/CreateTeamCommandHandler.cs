using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Teams.Commands.CreateTeam;

/// <summary>Handles <see cref="CreateTeamCommand"/>.</summary>
public sealed class CreateTeamCommandHandler(ITeamRepository repo)
    : IRequestHandler<CreateTeamCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(CreateTeamCommand request, CancellationToken ct)
    {
        var result = Team.Create(request.Name, request.Description);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);

        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
