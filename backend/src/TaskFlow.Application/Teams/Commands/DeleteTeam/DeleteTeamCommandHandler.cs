using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.DeleteTeam;

/// <summary>Handles <see cref="DeleteTeamCommand"/>.</summary>
public sealed class DeleteTeamCommandHandler(ITeamRepository repo)
    : IRequestHandler<DeleteTeamCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteTeamCommand request, CancellationToken ct)
    {
        var team = await repo.GetByIdAsync(request.TeamId, ct);
        if (team is null) return Result.Failure(TeamErrors.NotFound);

        repo.Remove(team);
        await repo.SaveChangesAsync(ct);

        return Result.Ok;
    }
}
