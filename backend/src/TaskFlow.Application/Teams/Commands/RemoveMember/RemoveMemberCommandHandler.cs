using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.RemoveMember;

/// <summary>Handles <see cref="RemoveMemberCommand"/>.</summary>
public sealed class RemoveMemberCommandHandler(ITeamRepository repo)
    : IRequestHandler<RemoveMemberCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        var team = await repo.GetByIdAsync(request.TeamId, ct);
        if (team is null) return Result.Failure(TeamErrors.NotFound);

        var result = team.RemoveMember(request.UserId);
        if (result.IsFailure) return result;

        repo.Update(team);
        await repo.SaveChangesAsync(ct);

        return Result.Ok;
    }
}
