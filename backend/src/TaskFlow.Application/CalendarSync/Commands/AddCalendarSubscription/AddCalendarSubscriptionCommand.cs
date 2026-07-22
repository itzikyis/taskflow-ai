using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CalendarSync.Commands.AddCalendarSubscription;

/// <summary>Creates a new external iCal subscription for a project.</summary>
/// <param name="ProjectId">The project to attach this subscription to.</param>
/// <param name="ExternalUrl">The public iCal feed URL to import events from.</param>
/// <param name="DisplayName">A human-readable label for the subscription.</param>
public sealed record AddCalendarSubscriptionCommand(
    Guid ProjectId,
    string ExternalUrl,
    string DisplayName) : IRequest<Result<Guid>>;
