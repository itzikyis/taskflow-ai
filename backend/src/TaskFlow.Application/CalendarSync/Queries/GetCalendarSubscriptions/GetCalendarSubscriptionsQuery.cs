using MediatR;
using TaskFlow.Application.CalendarSync.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CalendarSync.Queries.GetCalendarSubscriptions;

/// <summary>Returns all calendar subscriptions for a project.</summary>
/// <param name="ProjectId">The ID of the project whose subscriptions to retrieve.</param>
public sealed record GetCalendarSubscriptionsQuery(Guid ProjectId)
    : IRequest<Result<List<CalendarSubscriptionDto>>>;
