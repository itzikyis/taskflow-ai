using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CalendarSync.Commands.RemoveCalendarSubscription;

/// <summary>Handles <see cref="RemoveCalendarSubscriptionCommand"/>.</summary>
public sealed class RemoveCalendarSubscriptionCommandHandler(ICalendarSubscriptionRepository repository)
    : IRequestHandler<RemoveCalendarSubscriptionCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveCalendarSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await repository.GetByIdAsync(request.SubscriptionId, ct);
        if (sub is null)
            return Result.Failure(CalendarSubscriptionErrors.NotFound);

        repository.Remove(sub);
        await repository.SaveChangesAsync(ct);

        return Result.Ok;
    }
}
