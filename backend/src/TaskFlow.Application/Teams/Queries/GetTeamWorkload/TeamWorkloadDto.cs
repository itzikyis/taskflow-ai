namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Workload breakdown for a single team member.</summary>
public record MemberWorkloadDto(
    Guid UserId,
    string DisplayName,
    int OpenTasks,
    int InProgressTasks,
    int CompletedTasks,
    int TotalAssigned);

/// <summary>Aggregated workload data for all team members on a project.</summary>
public record TeamWorkloadDto(List<MemberWorkloadDto> Members, int UnassignedTasks);
