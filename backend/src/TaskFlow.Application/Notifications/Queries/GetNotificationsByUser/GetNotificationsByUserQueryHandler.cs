using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Notifications.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Queries.GetNotificationsByUser;

/// <summary>Handles <see cref="GetNotificationsByUserQuery"/>.</summary>
public sealed class GetNotificationsByUserQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetNotificationsByUserQuery, Result<IReadOnlyList<NotificationDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(
        GetNotificationsByUserQuery request,
        CancellationToken cancellationToken)
    {
        var notifications = await repository.GetByUserIdAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        IReadOnlyList<NotificationDto> dtos = notifications
            .Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.Title,
                n.Message,
                n.Type.ToString(),
                n.IsRead,
                n.RelatedEntityId,
                n.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<NotificationDto>>.Success(dtos);
    }
}
