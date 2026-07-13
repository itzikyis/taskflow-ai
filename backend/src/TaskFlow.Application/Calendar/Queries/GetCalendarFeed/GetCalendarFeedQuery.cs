using MediatR;

namespace TaskFlow.Application.Calendar.Queries.GetCalendarFeed;

/// <summary>
/// Query returning an iCalendar feed of a user's tasks (assigned to or created by
/// them) that have a due date. The user id acts as the secret feed token.
/// </summary>
public sealed record GetCalendarFeedQuery(Guid UserId) : IRequest<string>;
