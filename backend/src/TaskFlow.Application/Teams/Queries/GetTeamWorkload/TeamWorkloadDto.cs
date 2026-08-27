namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Workload breakdown for a single team member, including capacity utilisation.</summary>
public record MemberWorkloadDto(
    Guid UserId,
    string DisplayName,
    int OpenTasks,
    int InProgressTasks,
    int CompletedTasks,
    int TotalAssigned,
    /// <summary>Weekly capacity ceiling in hours (passed from the query).</summary>
    double CapacityHoursPerWeek,
    /// <summary>Hours logged against tasks this ISO week.</summary>
    double LoggedHoursThisWeek,
    /// <summary>LoggedHoursThisWeek / CapacityHoursPerWeek * 100, capped display-side.</summary>
    double UtilizationPercent,
    /// <summary>True when the member has logged more hours than their weekly capacity.</summary>
    bool OverCapacity);

/// <summary>Aggregated workload data for all team members on a project, including capacity totals.</summary>
public record TeamWorkloadDto(
    List<MemberWorkloadDto> Members,
    int UnassignedTasks,
    /// <summary>Sum of all members' capacity hours for the week.</summary>
    double TotalCapacityHoursPerWeek,
    /// <summary>Sum of all members' logged hours this week.</summary>
    double TotalLoggedHoursThisWeek);
