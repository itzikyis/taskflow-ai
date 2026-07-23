using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.AI.Queries.GetStatusDigest;

/// <summary>
/// Handles <see cref="GetStatusDigestQuery"/> by aggregating task data and delegating
/// narrative generation to the AI assistant.
/// </summary>
public sealed class GetStatusDigestQueryHandler(
    ITaskRepository taskRepository,
    IAiAssistantService ai,
    ILogger<GetStatusDigestQueryHandler> logger)
    : IRequestHandler<GetStatusDigestQuery, Result<StatusDigestDto>>
{
    /// <inheritdoc/>
    public async Task<Result<StatusDigestDto>> Handle(GetStatusDigestQuery request, CancellationToken ct)
    {
        var tasks = await taskRepository.GetAllAsync(cancellationToken: ct);

        var cutoff = DateTime.UtcNow.AddDays(-request.PeriodDays);
        var now = DateTime.UtcNow;

        var completed = tasks
            .Where(t => t.Status == TaskItemStatus.Done && t.UpdatedAt >= cutoff)
            .Select(t => t.Title)
            .ToList();

        var inProgress = tasks
            .Where(t => t.Status == TaskItemStatus.InProgress)
            .Select(t => t.Title)
            .ToList();

        var blockers = tasks
            .Where(t =>
                t.Status != TaskItemStatus.Done &&
                t.DueDate.HasValue &&
                t.DueDate.Value < now)
            .Select(t => t.Title)
            .ToList();

        var periodLabel = $"Last {request.PeriodDays} day{(request.PeriodDays == 1 ? "" : "s")}";

        try
        {
            var dto = await ai.GenerateStatusDigestAsync(
                periodLabel, completed, inProgress, blockers, ct);
            return Result<StatusDigestDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Status digest AI call failed: service is misconfigured.");
            return Result<StatusDigestDto>.Success(
                BuildFallback(periodLabel, completed, inProgress, blockers));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Status digest AI call failed due to an unexpected error.");
            return Result<StatusDigestDto>.Success(
                BuildFallback(periodLabel, completed, inProgress, blockers));
        }
    }

    private static StatusDigestDto BuildFallback(
        string periodLabel,
        List<string> completed,
        List<string> inProgress,
        List<string> blockers)
    {
        var health = blockers.Count > 5
            ? "Critical"
            : blockers.Count > 0 || (completed.Count == 0 && inProgress.Count > 0)
                ? "At Risk"
                : "Healthy";

        var narrative =
            $"During the {periodLabel.ToLowerInvariant()}, {completed.Count} task(s) were completed. " +
            $"There are currently {inProgress.Count} task(s) in progress" +
            (blockers.Count > 0
                ? $" and {blockers.Count} overdue task(s) that require immediate attention."
                : ".");

        return new StatusDigestDto(periodLabel, completed, inProgress, blockers, narrative, health);
    }
}
