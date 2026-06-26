using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Notifications.Commands.CreateNotification;

/// <summary>Handles <see cref="CreateNotificationCommand"/>.</summary>
public sealed class CreateNotificationCommandHandler(INotificationRepository repository)
    : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = Notification.Create(
            request.UserId,
            request.Title,
            request.Message,
            request.Type,
            request.RelatedEntityId);

        await repository.AddAsync(notification, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(notification.Id);
    }
}
