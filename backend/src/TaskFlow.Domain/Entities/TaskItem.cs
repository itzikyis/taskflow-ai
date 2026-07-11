using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root representing a task in the system.
/// </summary>
public sealed class TaskItem : AggregateRoot
{
    private TaskItem() { } // EF Core constructor

    private TaskItem(
        Guid id,
        string title,
        string? description,
        TaskPriority priority,
        Guid createdByUserId)
    {
        Id = id;
        Title = title;
        Description = description;
        Priority = priority;
        Status = TaskItemStatus.Todo;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskCreatedEvent(id, title, createdByUserId));
    }

    /// <summary>Gets the task title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the optional description.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the current status.</summary>
    public TaskItemStatus Status { get; private set; }

    /// <summary>Gets the task priority.</summary>
    public TaskPriority Priority { get; private set; }

    /// <summary>Gets the optional due date.</summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>Gets the optional assigned user ID.</summary>
    public Guid? AssignedToUserId { get; private set; }

    /// <summary>Gets the board column this task is placed in, or null if not on a board.</summary>
    public Guid? ColumnId { get; private set; }

    /// <summary>Gets the parent task this task is a subtask of, or null if it is top-level.</summary>
    public Guid? ParentTaskId { get; private set; }

    /// <summary>Gets the ID of the user who created this task.</summary>
    public Guid CreatedByUserId { get; private init; }

    /// <summary>Gets the UTC timestamp when the task was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the UTC timestamp of the last update.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Creates a new <see cref="TaskItem"/>.</summary>
    public static Result<TaskItem> Create(
        string title,
        string? description,
        TaskPriority priority,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<TaskItem>.Failure(TaskErrors.TitleRequired);

        if (title.Length > 200)
            return Result<TaskItem>.Failure(TaskErrors.TitleTooLong);

        var task = new TaskItem(Guid.NewGuid(), title.Trim(), description?.Trim(), priority, createdByUserId);
        return Result<TaskItem>.Success(task);
    }

    /// <summary>Updates the task title and description.</summary>
    public Result UpdateDetails(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(TaskErrors.TitleRequired);

        if (title.Length > 200)
            return Result.Failure(TaskErrors.TitleTooLong);

        Title = title.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }

    /// <summary>Assigns the task to a user.</summary>
    public void AssignTo(Guid userId)
    {
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new TaskAssignedEvent(Id, userId));
    }

    /// <summary>Sets the due date.</summary>
    public Result SetDueDate(DateTime dueDate)
    {
        if (dueDate <= DateTime.UtcNow)
            return Result.Failure(TaskErrors.DueDateInPast);

        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }

    /// <summary>Transitions the task to a new status.</summary>
    public Result TransitionTo(TaskItemStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
            return Result.Failure(TaskErrors.InvalidStatusTransition(Status, newStatus));

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }

    /// <summary>Places (or removes) the task in a board column.</summary>
    public void MoveToColumn(Guid? columnId)
    {
        ColumnId = columnId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Marks this task as a subtask of the given parent task.</summary>
    public void SetParent(Guid parentTaskId)
    {
        ParentTaskId = parentTaskId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidTransition(TaskItemStatus from, TaskItemStatus to) =>
        (from, to) switch
        {
            (TaskItemStatus.Todo, TaskItemStatus.InProgress) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Done) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Todo) => true,
            _ => false
        };
}
