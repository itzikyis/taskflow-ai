using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.TimeTracking.Dtos;

namespace TaskFlow.Application.TimeTracking.Queries.GetTaskTimeSummary;

/// <summary>Handles <see cref="GetTaskTimeSummaryQuery"/>.</summary>
public sealed class GetTaskTimeSummaryQueryHandler(ITimeEntryRepository repo)
    : IRequestHandler<GetTaskTimeSummaryQuery, TaskTimeSummaryDto>
{
    /// <inheritdoc/>
    public async Task<TaskTimeSummaryDto> Handle(GetTaskTimeSummaryQuery request, CancellationToken ct)
    {
        var entries = await repo.GetByTaskIdAsync(request.TaskId, ct);

        var dtos = entries
            .Select(e => new TimeEntryDto(e.Id, e.TaskId, e.UserId, e.Minutes, e.Note, e.LoggedAt))
            .ToList();

        return new TaskTimeSummaryDto(request.TaskId, dtos.Sum(e => e.Minutes), dtos);
    }
}
