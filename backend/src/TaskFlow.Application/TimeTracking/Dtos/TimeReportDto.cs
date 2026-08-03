namespace TaskFlow.Application.TimeTracking.Dtos;

/// <summary>Per-task breakdown row in a time report.</summary>
/// <param name="TaskId">The task identifier.</param>
/// <param name="TaskTitle">Human-readable task title.</param>
/// <param name="EstimatedMinutes">Original estimate in minutes (0 if not set).</param>
/// <param name="LoggedMinutes">Total minutes logged against this task within the report window.</param>
/// <param name="VarianceMinutes">LoggedMinutes − EstimatedMinutes; positive means over budget.</param>
public sealed record TaskTimeEntryDto(
    Guid TaskId,
    string TaskTitle,
    int EstimatedMinutes,
    int LoggedMinutes,
    int VarianceMinutes);

/// <summary>Aggregate estimate-vs-actual report for a project.</summary>
/// <param name="TotalEstimatedMinutes">Sum of estimates across all tasks.</param>
/// <param name="TotalLoggedMinutes">Sum of logged minutes across all tasks.</param>
/// <param name="VarianceMinutes">TotalLoggedMinutes − TotalEstimatedMinutes.</param>
/// <param name="TaskBreakdown">Per-task detail rows.</param>
public sealed record TimeReportDto(
    int TotalEstimatedMinutes,
    int TotalLoggedMinutes,
    int VarianceMinutes,
    List<TaskTimeEntryDto> TaskBreakdown);
