namespace TaskFlow.Application.Teams.Queries.GetAllTeams;

/// <summary>Data transfer object representing a team member.</summary>
public sealed record TeamMemberDto(Guid UserId, string Role, DateTime JoinedAt);

/// <summary>Data transfer object representing a team.</summary>
public sealed record TeamDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<TeamMemberDto> Members);
