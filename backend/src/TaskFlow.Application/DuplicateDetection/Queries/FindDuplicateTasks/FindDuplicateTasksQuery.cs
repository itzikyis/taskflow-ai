using MediatR;

namespace TaskFlow.Application.DuplicateDetection.Queries.FindDuplicateTasks;

/// <summary>DTO describing a possible duplicate task shown to the user.</summary>
public sealed record DuplicateMatchDto(Guid TaskId, string Title, double Score);

/// <summary>
/// Query that finds likely duplicates of a (possibly not-yet-created) task among
/// existing open tasks.
/// </summary>
/// <param name="Title">Candidate task title.</param>
/// <param name="Description">Optional candidate task description.</param>
/// <param name="ExcludeTaskId">Optional task id to exclude (e.g. the task itself).</param>
public sealed record FindDuplicateTasksQuery(
    string Title,
    string? Description,
    Guid? ExcludeTaskId = null) : IRequest<IReadOnlyList<DuplicateMatchDto>>;
