using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.MarkAsRead;

/// <summary>Handles <see cref="MarkAsReadCommand"/>.</summary>
public sealed class MarkAsReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<MarkAsReadCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        MarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound);

        if (notification.UserId != request.RequesterId)
            return Result.Failure(NotificationErrors.NotOwner);

        notification.MarkAsRead();
        repository.Update(notification);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
