using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>
/// Handles <see cref="TriageByContentQuery"/> by running duplicate detection then
/// asking the AI assistant for a priority suggestion.
/// </summary>
public sealed class TriageByContentQueryHandler(
    IAiAssistantService ai,
    IDuplicateTaskDetectionService duplicateDetector,
    ITaskRepository taskRepository,
    ILogger<TriageByContentQueryHandler> logger)
    : IRequestHandler<TriageByContentQuery, Result<TaskTriageResultDto>>
{
    private const int RecentTaskLimit = 50;
    private const double DuplicateThreshold = 0.35;

    /// <inheritdoc/>
    public async Task<Result<TaskTriageResultDto>> Handle(
        TriageByContentQuery request,
        CancellationToken ct)
    {
        // Load existing tasks to compare for duplicates.
        // GetAllAsync has no per-project filter; we take the most recent tasks as the context window.
        var allTasks = await taskRepository.GetAllAsync(cancellationToken: ct);
        var existingTasks = allTasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(RecentTaskLimit)
            .Select(t => (t.Id, t.Title, t.Description))
            .ToList();

        // Run duplicate detection (synchronous, text-similarity based).
        var matches = duplicateDetector.FindDuplicates(
            request.Title,
            request.Description,
            existingTasks,
            DuplicateThreshold);

        var potentialDuplicates = matches
            .Select(m => new PotentialDuplicateDto(m.TaskId, m.Title, m.Score))
            .ToList();

        // Ask AI for priority suggestion.
        try
        {
            var (priority, reasoning) = await ai.SuggestPriorityAsync(request.Title, request.Description, ct);

            return Result<TaskTriageResultDto>.Success(
                new TaskTriageResultDto(priority, reasoning, potentialDuplicates));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI priority suggestion failed: service is misconfigured.");
            return Result<TaskTriageResultDto>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI priority suggestion failed for title '{Title}'.", request.Title);
            // Graceful degradation: return duplicates with a neutral priority rather than failing.
            return Result<TaskTriageResultDto>.Success(
                new TaskTriageResultDto(
                    "Medium",
                    "AI priority suggestion is temporarily unavailable.",
                    potentialDuplicates));
        }
    }
}
