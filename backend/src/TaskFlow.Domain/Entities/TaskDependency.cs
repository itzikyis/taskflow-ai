using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A finish-to-start dependency: <see cref="TaskId"/> is blocked by
/// <see cref="BlockedByTaskId"/> (the prerequisite must finish first).
/// </summary>
public sealed class TaskDependency : AggregateRoot
{
    private TaskDependency() { }

    private TaskDependency(Guid id, Guid taskId, Guid blockedByTaskId)
    {
        Id = id;
        TaskId = taskId;
        BlockedByTaskId = blockedByTaskId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the task that is blocked (the dependent task).</summary>
    public Guid TaskId { get; private init; }

    /// <summary>Gets the prerequisite task that blocks it.</summary>
    public Guid BlockedByTaskId { get; private init; }

    /// <summary>Gets the UTC timestamp the dependency was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Creates a new dependency after basic validation.</summary>
    public static Result<TaskDependency> Create(Guid taskId, Guid blockedByTaskId)
    {
        if (taskId == blockedByTaskId)
            return Result<TaskDependency>.Failure(TaskDependencyErrors.SelfDependency);

        return Result<TaskDependency>.Success(new TaskDependency(Guid.NewGuid(), taskId, blockedByTaskId));
    }
}
