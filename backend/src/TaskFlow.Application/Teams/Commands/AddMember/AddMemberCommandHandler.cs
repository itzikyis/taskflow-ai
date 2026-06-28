using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.AddMember;

/// <summary>Handles <see cref="AddMemberCommand"/>.</summary>
public sealed class AddMemberCommandHandler(ITeamRepository repo)
    : IRequestHandler<AddMemberCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(AddMemberCommand request, CancellationToken ct)
    {
        var team = await repo.GetByIdAsync(request.TeamId, ct);
        if (team is null) return Result.Failure(TeamErrors.NotFound);

        var result = team.AddMember(request.UserId, request.Role);
        if (result.IsFailure) return result;

        repo.Update(team);
        await repo.SaveChangesAsync(ct);

        return Result.Ok;
    }
}
