namespace TaskFlow.Application.TimeTracking.Dtos;

/// <summary>Hours logged per day of the week (Mon–Sun, 7 values) for a single task.</summary>
public sealed record TimesheetRowDto(Guid TaskId, string TaskTitle, decimal[] HoursByDay);

/// <summary>Weekly timesheet for a user: one row per task with time logged, totals per day, and a grand total.</summary>
public sealed record TimesheetDto(
    DateOnly WeekStart,
    List<TimesheetRowDto> Rows,
    decimal[] TotalByDay,
    decimal GrandTotal);
