using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Reporting.Queries.GetProjectAnalytics;

/// <summary>Handles <see cref="GetProjectAnalyticsQuery"/> — computes velocity and status breakdown.</summary>
public sealed class GetProjectAnalyticsQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetProjectAnalyticsQuery, Result<ProjectAnalyticsDto>>
{
    private const int VelocityWeeks = 6;

    /// <inheritdoc/>
    public async Task<Result<ProjectAnalyticsDto>> Handle(
        GetProjectAnalyticsQuery request,
        CancellationToken ct)
    {
        var tasks = await taskRepository.GetAllAsync(null, ct);

        var now = DateTime.UtcNow;

        var total      = tasks.Count;
        var completed  = tasks.Count(t => t.Status == TaskItemStatus.Done);
        var inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var open       = tasks.Count(t => t.Status == TaskItemStatus.Todo);
        var overdue    = tasks.Count(t =>
            t.Status != TaskItemStatus.Done &&
            t.DueDate.HasValue &&
            t.DueDate.Value < now);

        var weeklyVelocity = BuildWeeklyVelocity(tasks, now, VelocityWeeks);

        return Result<ProjectAnalyticsDto>.Success(new ProjectAnalyticsDto(
            TotalTasks:      total,
            CompletedTasks:  completed,
            InProgressTasks: inProgress,
            OpenTasks:       open,
            OverdueTasks:    overdue,
            WeeklyVelocity:  weeklyVelocity));
    }

    private static List<WeeklyVelocityDto> BuildWeeklyVelocity(
        IReadOnlyList<Domain.Entities.TaskItem> tasks,
        DateTime now,
        int weeks)
    {
        // Anchor to the Monday of the current ISO week.
        var currentMonday = now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7));
        var windowStart   = currentMonday.AddDays(-7 * (weeks - 1));

        var buckets = Enumerable.Range(0, weeks)
            .Select(i =>
            {
                var monday = windowStart.AddDays(i * 7);
                return (monday, label: $"W{monday:MM/dd}");
            })
            .ToList();

        var completedInWindow = tasks
            .Where(t =>
                t.Status == TaskItemStatus.Done &&
                (t.UpdatedAt ?? t.CreatedAt).ToUniversalTime().Date >= windowStart)
            .ToList();

        return buckets
            .Select(b =>
            {
                var sunday = b.monday.AddDays(6);
                var count  = completedInWindow.Count(t =>
                {
                    var date = (t.UpdatedAt ?? t.CreatedAt).ToUniversalTime().Date;
                    return date >= b.monday && date <= sunday;
                });
                return new WeeklyVelocityDto(b.label, count);
            })
            .ToList();
    }
}
