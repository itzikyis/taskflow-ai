using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.MarkAllAsRead;

/// <summary>Handles <see cref="MarkAllAsReadCommand"/>.</summary>
public sealed class MarkAllAsReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<MarkAllAsReadCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        MarkAllAsReadCommand request,
        CancellationToken cancellationToken)
    {
        // Load all notifications for the user (large page to capture unread ones)
        var notifications = await repository.GetByUserIdAsync(
            request.UserId, page: 1, pageSize: int.MaxValue, cancellationToken);

        var unread = notifications.Where(n => !n.IsRead).ToList();

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
            repository.Update(notification);
        }

        if (unread.Count > 0)
            await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
