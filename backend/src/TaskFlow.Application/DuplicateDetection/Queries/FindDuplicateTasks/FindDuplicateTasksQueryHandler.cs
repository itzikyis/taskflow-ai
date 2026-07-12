using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.DuplicateDetection.Queries.FindDuplicateTasks;

/// <summary>Handles <see cref="FindDuplicateTasksQuery"/>.</summary>
public sealed class FindDuplicateTasksQueryHandler(
    ITaskRepository taskRepository,
    IDuplicateTaskDetectionService detector)
    : IRequestHandler<FindDuplicateTasksQuery, IReadOnlyList<DuplicateMatchDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DuplicateMatchDto>> Handle(
        FindDuplicateTasksQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return [];

        var all = await taskRepository.GetAllAsync(null, ct);

        // Compare against open (non-Done) tasks only, excluding the task itself.
        var candidates = all
            .Where(t => t.Status != TaskItemStatus.Done)
            .Where(t => request.ExcludeTaskId is null || t.Id != request.ExcludeTaskId)
            .Select(t => (t.Id, t.Title, t.Description));

        return detector
            .FindDuplicates(request.Title, request.Description, candidates)
            .Select(m => new DuplicateMatchDto(m.TaskId, m.Title, m.Score))
            .ToList();
    }
}
