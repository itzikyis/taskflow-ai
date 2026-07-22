using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CalendarSync.Commands.SyncCalendarSubscription;

/// <summary>Imports events from the subscription's external iCal feed and creates or updates tasks.</summary>
/// <param name="SubscriptionId">The ID of the subscription to sync.</param>
/// <param name="CreatedByUserId">The user ID to assign as creator for any newly created tasks.</param>
public sealed record SyncCalendarSubscriptionCommand(
    Guid SubscriptionId,
    Guid CreatedByUserId) : IRequest<Result<int>>;
