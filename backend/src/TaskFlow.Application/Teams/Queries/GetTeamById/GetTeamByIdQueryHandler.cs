using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Teams.Queries.GetAllTeams;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Queries.GetTeamById;

/// <summary>Handles <see cref="GetTeamByIdQuery"/>.</summary>
public sealed class GetTeamByIdQueryHandler(ITeamRepository repo)
    : IRequestHandler<GetTeamByIdQuery, Result<TeamDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TeamDto>> Handle(GetTeamByIdQuery request, CancellationToken ct)
    {
        var team = await repo.GetByIdAsync(request.TeamId, ct);
        if (team is null) return Result<TeamDto>.Failure(TeamErrors.NotFound);

        var dto = new TeamDto(
            team.Id,
            team.Name,
            team.Description,
            team.CreatedAt,
            team.Members.Select(m => new TeamMemberDto(m.UserId, m.Role.ToString(), m.JoinedAt)).ToList()
        );

        return Result<TeamDto>.Success(dto);
    }
}
