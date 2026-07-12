using MediatR;
using TaskFlow.Application.Tasks.Queries.GetTaskById;

namespace TaskFlow.Application.Search.Queries.SearchTasks;

/// <summary>Result of a natural-language task search.</summary>
public sealed record SearchTasksResultDto(
    string Interpretation,
    IReadOnlyList<TaskDto> Results);

/// <summary>Natural-language task search query.</summary>
/// <param name="Query">The raw free-text query.</param>
/// <param name="CurrentUserId">The authenticated caller (for "assigned to me" style queries).</param>
public sealed record SearchTasksQuery(string Query, Guid CurrentUserId)
    : IRequest<SearchTasksResultDto>;
