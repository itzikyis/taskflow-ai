using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Reporting.Dtos;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Reporting.Queries.GetDashboardMetrics;

/// <summary>Handles <see cref="GetDashboardMetricsQuery"/> as a read-only aggregation over tasks.</summary>
public sealed class GetDashboardMetricsQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    /// <inheritdoc/>
    public async Task<DashboardMetricsDto> Handle(GetDashboardMetricsQuery request, CancellationToken ct)
    {
        var tasks = await taskRepository.GetAllAsync(null, ct);

        var workload = tasks
            .Where(t => t.Status != TaskItemStatus.Done)
            .GroupBy(t => t.AssignedToUserId)
            .Select(g => new WorkloadItemDto(g.Key, g.Count()))
            .OrderByDescending(w => w.OpenTasks)
            .ToList();

        // Throughput/burndown proxy: tasks completed per day (by last-updated date).
        var completionTrend = tasks
            .Where(t => t.Status == TaskItemStatus.Done)
            .GroupBy(t => DateOnly.FromDateTime((t.UpdatedAt ?? t.CreatedAt).ToUniversalTime()))
            .OrderBy(g => g.Key)
            .Select(g => new CompletionPointDto(g.Key.ToString("yyyy-MM-dd"), g.Count()))
            .ToList();

        return new DashboardMetricsDto(
            Total: tasks.Count,
            Todo: tasks.Count(t => t.Status == TaskItemStatus.Todo),
            InProgress: tasks.Count(t => t.Status == TaskItemStatus.InProgress),
            Done: tasks.Count(t => t.Status == TaskItemStatus.Done),
            Low: Count(tasks, TaskPriority.Low),
            Medium: Count(tasks, TaskPriority.Medium),
            High: Count(tasks, TaskPriority.High),
            Critical: Count(tasks, TaskPriority.Critical),
            Workload: workload,
            CompletionTrend: completionTrend);
    }

    private static int Count(IReadOnlyList<TaskItem> tasks, TaskPriority priority) =>
        tasks.Count(t => t.Priority == priority);
}
