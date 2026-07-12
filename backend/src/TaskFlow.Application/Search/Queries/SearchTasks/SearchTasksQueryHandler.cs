using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Tasks.Queries.GetTaskById;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Search.Queries.SearchTasks;

/// <summary>Handles <see cref="SearchTasksQuery"/> by interpreting the text and filtering tasks.</summary>
public sealed class SearchTasksQueryHandler(
    ITaskRepository taskRepository,
    ITaskSearchInterpreter interpreter)
    : IRequestHandler<SearchTasksQuery, SearchTasksResultDto>
{
    /// <inheritdoc/>
    public async Task<SearchTasksResultDto> Handle(SearchTasksQuery request, CancellationToken ct)
    {
        var filter = interpreter.Interpret(request.Query ?? string.Empty);
        var all = await taskRepository.GetAllAsync(null, ct);

        var results = all
            .Where(t => Matches(t, filter, request.CurrentUserId))
            .Select(ToDto)
            .ToList();

        return new SearchTasksResultDto(interpreter.Describe(filter), results);
    }

    private static bool Matches(TaskItem task, TaskSearchFilter filter, Guid userId)
    {
        if (filter.Status is { } status && task.Status != status)
            return false;

        if (filter.OpenOnly && task.Status == TaskItemStatus.Done)
            return false;

        if (filter.Priority is { } priority && task.Priority != priority)
            return false;

        if (filter.Overdue &&
            !(task.DueDate is { } due && due < DateTime.UtcNow && task.Status != TaskItemStatus.Done))
            return false;

        if (filter.MineOnly && task.AssignedToUserId != userId && task.CreatedByUserId != userId)
            return false;

        if (filter.Keywords.Count > 0)
        {
            var haystack = $"{task.Title} {task.Description}".ToLowerInvariant();
            if (!filter.Keywords.Any(k => haystack.Contains(k)))
                return false;
        }

        return true;
    }

    private static TaskDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
        t.AssignedToUserId, t.CreatedByUserId, t.CreatedAt, t.UpdatedAt, t.ColumnId, t.ParentTaskId);
}
