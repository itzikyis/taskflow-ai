using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.UpdateMemberRole;

/// <summary>Handles <see cref="UpdateMemberRoleCommand"/>.</summary>
public sealed class UpdateMemberRoleCommandHandler(ITeamRepository repo)
    : IRequestHandler<UpdateMemberRoleCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(UpdateMemberRoleCommand request, CancellationToken ct)
    {
        var team = await repo.GetByIdAsync(request.TeamId, ct);
        if (team is null) return Result.Failure(TeamErrors.NotFound);

        var result = team.UpdateMemberRole(request.UserId, request.Role);
        if (result.IsFailure) return result;

        repo.Update(team);
        await repo.SaveChangesAsync(ct);

        return Result.Ok;
    }
}
