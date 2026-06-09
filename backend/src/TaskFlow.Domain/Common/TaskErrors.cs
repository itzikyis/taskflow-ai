using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Common;

/// <summary>Domain errors specific to the Task aggregate.</summary>
public static class TaskErrors
{
    public static readonly Error TitleRequired =
        new("Task.TitleRequired", "Task title is required.");

    public static readonly Error TitleTooLong =
        new("Task.TitleTooLong", "Task title must not exceed 200 characters.");

    public static readonly Error DueDateInPast =
        new("Task.DueDateInPast", "Due date must be in the future.");

    public static readonly Error NotFound =
        new("Task.NotFound", "The requested task was not found.");

    public static Error InvalidStatusTransition(TaskItemStatus from, TaskItemStatus to) =>
        new("Task.InvalidTransition", $"Cannot transition task from '{from}' to '{to}'.");
}
