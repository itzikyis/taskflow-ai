using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.RenameTeam;

/// <summary>Handles <see cref="RenameTeamCommand"/>.</summary>
public sealed class RenameTeamCommandHandler(ITeamRepository repo)
    : IRequestHandler<RenameTeamCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RenameTeamCommand request, CancellationToken ct)
    {
        var team = await repo.GetByIdAsync(request.TeamId, ct);
        if (team is null) return Result.Failure(TeamErrors.NotFound);

        var result = team.Rename(request.Name);
        if (result.IsFailure) return result;

        await repo.SaveChangesAsync(ct);

        return Result.Ok;
    }
}
