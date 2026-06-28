using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Queries.GetUnreadCount;

/// <summary>Handles <see cref="GetUnreadCountQuery"/>.</summary>
public sealed class GetUnreadCountQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    /// <inheritdoc/>
    public async Task<Result<int>> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await repository.GetUnreadCountAsync(request.UserId, cancellationToken);
        return Result<int>.Success(count);
    }
}
