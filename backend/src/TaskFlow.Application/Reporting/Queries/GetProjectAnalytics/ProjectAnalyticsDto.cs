namespace TaskFlow.Application.Reporting.Queries.GetProjectAnalytics;

/// <summary>Completed-task count for a single calendar week.</summary>
public record WeeklyVelocityDto(string WeekLabel, int Completed);

/// <summary>Aggregate analytics for a project: status counts and weekly velocity.</summary>
public record ProjectAnalyticsDto(
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int OpenTasks,
    int OverdueTasks,
    List<WeeklyVelocityDto> WeeklyVelocity);
