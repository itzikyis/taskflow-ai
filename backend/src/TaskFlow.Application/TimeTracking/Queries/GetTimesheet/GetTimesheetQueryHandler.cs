using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.TimeTracking.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Queries.GetTimesheet;

/// <summary>Handles <see cref="GetTimesheetQuery"/> by aggregating time entries into a weekly grid.</summary>
public sealed class GetTimesheetQueryHandler(
    ITimeEntryRepository timeEntryRepo,
    ITaskRepository taskRepo)
    : IRequestHandler<GetTimesheetQuery, Result<TimesheetDto>>
{
    private const int DaysInWeek = 7;

    /// <inheritdoc/>
    public async Task<Result<TimesheetDto>> Handle(GetTimesheetQuery request, CancellationToken ct)
    {
        // Build inclusive UTC bounds for the week (Mon 00:00:00 → Sun 23:59:59.999…)
        var weekStart = request.WeekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var weekEnd   = weekStart.AddDays(DaysInWeek).AddTicks(-1);

        var entries = await timeEntryRepo.GetByUserAndDateRangeAsync(
            request.UserId, weekStart, weekEnd, ct);

        // Group entries by task
        var byTask = entries.GroupBy(e => e.TaskId);

        var rows = new List<TimesheetRowDto>();

        foreach (var taskGroup in byTask)
        {
            var task = await taskRepo.GetByIdAsync(taskGroup.Key, ct);
            var title = task?.Title ?? taskGroup.Key.ToString();

            var hoursByDay = new decimal[DaysInWeek];

            foreach (var entry in taskGroup)
            {
                // LoggedAt is UTC; day-of-week index relative to week start (Mon = 0)
                var dayIndex = (int)(entry.LoggedAt.Date - weekStart.Date).TotalDays;
                if (dayIndex >= 0 && dayIndex < DaysInWeek)
                    hoursByDay[dayIndex] += Math.Round(entry.Minutes / 60m, 2);
            }

            rows.Add(new TimesheetRowDto(taskGroup.Key, title, hoursByDay));
        }

        // Column totals
        var totalByDay = new decimal[DaysInWeek];
        for (var d = 0; d < DaysInWeek; d++)
            totalByDay[d] = rows.Sum(r => r.HoursByDay[d]);

        var grandTotal = totalByDay.Sum();

        return Result<TimesheetDto>.Success(
            new TimesheetDto(request.WeekStart, rows, totalByDay, grandTotal));
    }
}
