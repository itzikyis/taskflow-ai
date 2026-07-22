using MediatR;
using TaskFlow.Application.CalendarSync.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CalendarSync.Queries.GetCalendarSubscriptions;

/// <summary>Handles <see cref="GetCalendarSubscriptionsQuery"/>.</summary>
public sealed class GetCalendarSubscriptionsQueryHandler(ICalendarSubscriptionRepository repository)
    : IRequestHandler<GetCalendarSubscriptionsQuery, Result<List<CalendarSubscriptionDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<List<CalendarSubscriptionDto>>> Handle(
        GetCalendarSubscriptionsQuery request,
        CancellationToken ct)
    {
        var subscriptions = await repository.GetByProjectAsync(request.ProjectId, ct);

        var dtos = subscriptions
            .Select(s => new CalendarSubscriptionDto(s.Id, s.ProjectId, s.ExternalUrl, s.DisplayName, s.LastSyncedAt))
            .ToList();

        return Result<List<CalendarSubscriptionDto>>.Success(dtos);
    }
}
