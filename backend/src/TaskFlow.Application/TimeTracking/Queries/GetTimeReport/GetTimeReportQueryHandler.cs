using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.TimeTracking.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Queries.GetTimeReport;

/// <summary>Handles <see cref="GetTimeReportQuery"/> by joining project tasks with their logged time.</summary>
public sealed class GetTimeReportQueryHandler(
    ITaskRepository taskRepo,
    ITimeEntryRepository timeEntryRepo)
    : IRequestHandler<GetTimeReportQuery, Result<TimeReportDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TimeReportDto>> Handle(GetTimeReportQuery request, CancellationToken ct)
    {
        var tasks = await taskRepo.GetByProjectIdAsync(request.ProjectId, ct);

        if (tasks.Count == 0)
        {
            return Result<TimeReportDto>.Success(
                new TimeReportDto(0, 0, 0, []));
        }

        var taskIds = tasks.Select(t => t.Id);

        var entries = await timeEntryRepo.GetByTaskIdsAsync(taskIds, request.From, request.To, ct);

        var loggedByTask = entries
            .GroupBy(e => e.TaskId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Minutes));

        var breakdown = tasks.Select(t =>
        {
            var estimated = 0; // TaskItem has no EstimatedMinutes field
            var logged    = loggedByTask.GetValueOrDefault(t.Id, 0);
            return new TaskTimeEntryDto(t.Id, t.Title, estimated, logged, logged - estimated);
        }).ToList();

        var totalEstimated = breakdown.Sum(r => r.EstimatedMinutes);
        var totalLogged    = breakdown.Sum(r => r.LoggedMinutes);

        return Result<TimeReportDto>.Success(
            new TimeReportDto(totalEstimated, totalLogged, totalLogged - totalEstimated, breakdown));
    }
}
