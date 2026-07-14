namespace TaskFlow.Application.Initiatives.Dtos;

/// <summary>Read model for a single initiative.</summary>
public sealed record InitiativeDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    string Priority,
    string Labels,
    DateTime? StartDate,
    DateTime? TargetDate,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    IReadOnlyList<Guid> ProjectIds,
    int TotalTasks,
    int CompletedTasks);
