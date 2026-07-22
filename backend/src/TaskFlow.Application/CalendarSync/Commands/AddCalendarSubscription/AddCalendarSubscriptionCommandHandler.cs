using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.CalendarSync.Commands.AddCalendarSubscription;

/// <summary>Handles <see cref="AddCalendarSubscriptionCommand"/>.</summary>
public sealed class AddCalendarSubscriptionCommandHandler(ICalendarSubscriptionRepository repository)
    : IRequestHandler<AddCalendarSubscriptionCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(AddCalendarSubscriptionCommand request, CancellationToken ct)
    {
        var result = CalendarSubscription.Create(request.ProjectId, request.ExternalUrl, request.DisplayName);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await repository.AddAsync(result.Value!, ct);
        await repository.SaveChangesAsync(ct);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
