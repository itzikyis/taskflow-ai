using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.DevelopmentLinks.Commands.IngestGitHubEvent;

/// <summary>
/// Handles <see cref="IngestGitHubEventCommand"/>: parses the payload, detects
/// task references, upserts a development link for each referenced task, and
/// auto-transitions task status when a pull request is merged.
/// </summary>
public sealed class IngestGitHubEventCommandHandler(
    IGitHubWebhookParser parser,
    IDevelopmentLinkRepository repo,
    ITaskRepository taskRepository,
    ILogger<IngestGitHubEventCommandHandler> logger)
    : IRequestHandler<IngestGitHubEventCommand, Result<int>>
{
    /// <inheritdoc/>
    public async Task<Result<int>> Handle(IngestGitHubEventCommand request, CancellationToken ct)
    {
        IReadOnlyList<GitHub.ParsedDevelopmentRef> refs;
        try
        {
            refs = parser.Parse(request.EventType, request.JsonPayload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse GitHub webhook payload for event '{EventType}'.", request.EventType);
            return Result<int>.Failure(new Error("GitHub.InvalidPayload", "The webhook payload could not be parsed."));
        }

        var upserted = 0;

        foreach (var reference in refs)
        {
            var taskIds = TaskReferenceExtractor.Extract(reference.TextToScan);
            if (taskIds.Count == 0)
                continue;

            foreach (var taskId in taskIds)
            {
                // Only link references to tasks that actually exist.
                var task = await taskRepository.GetByIdAsync(taskId, ct);
                if (task is null)
                    continue;

                if (!string.IsNullOrWhiteSpace(reference.ExternalId))
                {
                    var existing = await repo.FindByExternalRefAsync(
                        taskId, reference.Repository, reference.ExternalId, ct);

                    if (existing is not null)
                    {
                        existing.Update(reference.Status, reference.Title);
                        upserted++;
                        TryAutoTransition(task, reference.Status);
                        continue;
                    }
                }

                var created = TaskDevelopmentLink.Create(
                    taskId, reference.Repository, reference.RefType,
                    reference.Title, reference.Url, reference.Status, reference.ExternalId);

                if (created.IsFailure)
                    continue;

                await repo.AddAsync(created.Value!, ct);
                upserted++;
                TryAutoTransition(task, reference.Status);
            }
        }

        if (upserted > 0)
            await repo.SaveChangesAsync(ct);

        return Result<int>.Success(upserted);
    }

    /// <summary>
    /// Attempts to auto-transition the task to <see cref="TaskItemStatus.InReview"/> when a
    /// linked pull request is merged. The transition is a best-effort operation: if the task
    /// is not in a state that allows the transition (e.g. already Done) it is silently skipped.
    /// </summary>
    private void TryAutoTransition(TaskItem task, DevelopmentLinkStatus linkStatus)
    {
        if (linkStatus != DevelopmentLinkStatus.Merged)
            return;

        var result = task.TransitionTo(TaskItemStatus.InReview);
        if (result.IsFailure)
        {
            logger.LogDebug(
                "Could not auto-transition task {TaskId} to InReview: {Error}",
                task.Id, result.Error.Description);
        }
        else
        {
            taskRepository.Update(task);
            logger.LogInformation(
                "Auto-transitioned task {TaskId} to InReview after PR merge.", task.Id);
        }
    }
}
