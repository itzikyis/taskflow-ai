using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.DevelopmentLinks.Commands.IngestGitHubEvent;

/// <summary>
/// Handles <see cref="IngestGitHubEventCommand"/>: parses the payload, detects
/// task references, and upserts a development link for each referenced task.
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
                if (await taskRepository.GetByIdAsync(taskId, ct) is null)
                    continue;

                if (!string.IsNullOrWhiteSpace(reference.ExternalId))
                {
                    var existing = await repo.FindByExternalRefAsync(
                        taskId, reference.Repository, reference.ExternalId, ct);

                    if (existing is not null)
                    {
                        existing.Update(reference.Status, reference.Title);
                        upserted++;
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
            }
        }

        if (upserted > 0)
            await repo.SaveChangesAsync(ct);

        return Result<int>.Success(upserted);
    }
}
