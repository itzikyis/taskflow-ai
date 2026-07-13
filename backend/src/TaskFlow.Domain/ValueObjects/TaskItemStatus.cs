namespace TaskFlow.Domain.ValueObjects;

/// <summary>Lifecycle status of a <see cref="TaskFlow.Domain.Entities.TaskItem"/>.</summary>
public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    InReview = 3
}
