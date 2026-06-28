using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Queries.GetAllTeams;

/// <summary>Handles <see cref="GetAllTeamsQuery"/>.</summary>
public sealed class GetAllTeamsQueryHandler(ITeamRepository repo)
    : IRequestHandler<GetAllTeamsQuery, Result<IReadOnlyList<TeamDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<TeamDto>>> Handle(GetAllTeamsQuery request, CancellationToken ct)
    {
        var teams = await repo.GetAllAsync(ct);

        var dtos = teams.Select(t => new TeamDto(
            t.Id,
            t.Name,
            t.Description,
            t.CreatedAt,
            t.Members.Select(m => new TeamMemberDto(m.UserId, m.Role.ToString(), m.JoinedAt)).ToList()
        )).ToList();

        return Result<IReadOnlyList<TeamDto>>.Success(dtos);
    }
}
