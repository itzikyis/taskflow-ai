using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Queries.GetTaskById;

/// <summary>Read-model DTO returned by task queries.</summary>
public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
