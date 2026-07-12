namespace TaskFlow.Application.Reporting.Dtos;

/// <summary>Open-task load for a single assignee (null id = unassigned).</summary>
public sealed record WorkloadItemDto(Guid? UserId, int OpenTasks);

/// <summary>Number of tasks completed on a given day (for a burndown/throughput view).</summary>
public sealed record CompletionPointDto(string Date, int Completed);

/// <summary>Aggregate project analytics derived from the current task set.</summary>
public sealed record DashboardMetricsDto(
    int Total,
    int Todo,
    int InProgress,
    int Done,
    int Low,
    int Medium,
    int High,
    int Critical,
    IReadOnlyList<WorkloadItemDto> Workload,
    IReadOnlyList<CompletionPointDto> CompletionTrend);
