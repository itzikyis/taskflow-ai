using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CalendarSync.Commands.RemoveCalendarSubscription;

/// <summary>Removes a calendar subscription by its identifier.</summary>
/// <param name="SubscriptionId">The ID of the subscription to remove.</param>
public sealed record RemoveCalendarSubscriptionCommand(Guid SubscriptionId) : IRequest<Result>;
