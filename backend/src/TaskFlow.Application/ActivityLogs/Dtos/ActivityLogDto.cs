namespace TaskFlow.Application.ActivityLogs.Dtos;

/// <summary>Data transfer object representing a single activity log entry.</summary>
public sealed record ActivityLogDto(
    Guid Id,
    Guid ActorId,
    string Action,
    string EntityType,
    Guid EntityId,
    string? EntityName,
    Guid? ProjectId,
    string? Metadata,
    DateTime OccurredAt);
