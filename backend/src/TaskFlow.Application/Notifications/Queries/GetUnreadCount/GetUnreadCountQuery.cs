using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Queries.GetUnreadCount;

/// <summary>Query to retrieve the number of unread notifications for a user.</summary>
/// <param name="UserId">The user to check unread count for.</param>
public sealed record GetUnreadCountQuery(Guid UserId) : IRequest<Result<int>>;
