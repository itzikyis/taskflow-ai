using MediatR;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Calendar.Queries.GetCalendarFeed;

/// <summary>Handles <see cref="GetCalendarFeedQuery"/>.</summary>
public sealed class GetCalendarFeedQueryHandler(
    ITaskRepository taskRepository,
    ICalendarFeedBuilder builder)
    : IRequestHandler<GetCalendarFeedQuery, string>
{
    /// <inheritdoc/>
    public async Task<string> Handle(GetCalendarFeedQuery request, CancellationToken ct)
    {
        var all = await taskRepository.GetAllAsync(null, ct);

        var calendarTasks = all
            .Where(t => t.DueDate is not null)
            .Where(t => t.AssignedToUserId == request.UserId || t.CreatedByUserId == request.UserId)
            .Select(t => new CalendarTask(
                t.Id, t.Title, t.Description, t.Status.ToString(), t.Priority.ToString(), t.DueDate!.Value));

        return builder.Build(calendarTasks);
    }
}
