namespace TaskFlow.Application.TimeTracking.Dtos;

/// <summary>DTO representing a single logged time entry.</summary>
public sealed record TimeEntryDto(
    Guid Id,
    Guid TaskId,
    Guid UserId,
    int Minutes,
    string? Note,
    DateTime LoggedAt);

/// <summary>Time entries for a task plus their total.</summary>
public sealed record TaskTimeSummaryDto(
    Guid TaskId,
    int TotalMinutes,
    IReadOnlyList<TimeEntryDto> Entries);
