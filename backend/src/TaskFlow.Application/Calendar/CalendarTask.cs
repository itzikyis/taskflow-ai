namespace TaskFlow.Application.Calendar;

/// <summary>A task projected into a calendar event (must have a due date).</summary>
public sealed record CalendarTask(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime DueDate);
