namespace TaskFlow.Application.Goals.Queries.GetGoalsByProject;

/// <summary>Represents a single key result returned in a query response.</summary>
public record KeyResultDto(
    Guid Id,
    string Title,
    decimal TargetValue,
    decimal CurrentValue,
    string Unit,
    int ProgressPercent);

/// <summary>Represents a goal (objective) returned in a query response.</summary>
public record GoalDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    int ProgressPercent,
    DateTime? DueDate,
    List<KeyResultDto> KeyResults);
