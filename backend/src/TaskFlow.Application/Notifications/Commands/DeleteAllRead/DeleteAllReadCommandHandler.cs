using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.DeleteAllRead;

/// <summary>Handles <see cref="DeleteAllReadCommand"/>.</summary>
public sealed class DeleteAllReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<DeleteAllReadCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        DeleteAllReadCommand request,
        CancellationToken cancellationToken)
    {
        var notifications = await repository.GetByUserIdAsync(
            request.UserId, page: 1, pageSize: int.MaxValue, cancellationToken);

        var readNotifications = notifications.Where(n => n.IsRead).ToList();

        if (readNotifications.Count > 0)
        {
            repository.RemoveRange(readNotifications);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok;
    }
}
