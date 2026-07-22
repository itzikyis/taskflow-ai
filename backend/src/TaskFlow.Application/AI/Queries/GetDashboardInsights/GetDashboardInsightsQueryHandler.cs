using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.AI.Queries.GetDashboardInsights;

/// <summary>Handles <see cref="GetDashboardInsightsQuery"/> by computing dashboard metrics and delegating narrative generation to the AI assistant.</summary>
public sealed class GetDashboardInsightsQueryHandler(
    ITaskRepository taskRepository,
    IAiAssistantService ai,
    ILogger<GetDashboardInsightsQueryHandler> logger)
    : IRequestHandler<GetDashboardInsightsQuery, Result<DashboardInsightsDto>>
{
    /// <inheritdoc/>
    public async Task<Result<DashboardInsightsDto>> Handle(GetDashboardInsightsQuery request, CancellationToken ct)
    {
        var tasks = await taskRepository.GetAllAsync(cancellationToken: ct);

        var now = DateTime.UtcNow;
        var total = tasks.Count;
        var completed = tasks.Count(t => t.Status == TaskItemStatus.Done);
        var inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var overdue = tasks.Count(t =>
            t.Status != TaskItemStatus.Done &&
            t.DueDate.HasValue &&
            t.DueDate.Value < now);

        try
        {
            var dto = await ai.GenerateDashboardInsightsAsync(total, completed, inProgress, overdue, ct);
            return Result<DashboardInsightsDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Dashboard insights AI call failed: service is misconfigured.");
            return Result<DashboardInsightsDto>.Success(BuildFallback(total, completed, inProgress, overdue));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dashboard insights AI call failed due to an unexpected error.");
            return Result<DashboardInsightsDto>.Success(BuildFallback(total, completed, inProgress, overdue));
        }
    }

    private static DashboardInsightsDto BuildFallback(int total, int completed, int inProgress, int overdue)
    {
        var completionPct = total == 0 ? 0 : (int)Math.Round(completed * 100.0 / total);
        var health = overdue > 5 || (total > 0 && completionPct < 20)
            ? "Critical"
            : overdue > 0 || (total > 0 && completionPct < 50)
                ? "At Risk"
                : "Healthy";

        var narrative =
            $"The project has {total} tasks in total, {completed} of which are completed ({completionPct}%). " +
            $"There are currently {inProgress} tasks in progress" +
            (overdue > 0 ? $" and {overdue} overdue task(s) that need attention." : ".");

        var highlights = new List<string>
        {
            $"{completed}/{total} tasks completed ({completionPct}%)",
            $"{inProgress} task(s) in progress",
        };

        if (overdue > 0)
            highlights.Add($"{overdue} overdue task(s) require immediate attention");

        return new DashboardInsightsDto(narrative, highlights, health);
    }
}
