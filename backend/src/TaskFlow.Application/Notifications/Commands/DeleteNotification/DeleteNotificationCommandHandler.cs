using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.DeleteNotification;

/// <summary>Handles <see cref="DeleteNotificationCommand"/>.</summary>
public sealed class DeleteNotificationCommandHandler(INotificationRepository repository)
    : IRequestHandler<DeleteNotificationCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound);

        if (notification.UserId != request.RequesterId)
            return Result.Failure(NotificationErrors.NotOwner);

        repository.RemoveRange([notification]);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
