using MediatR;
using TaskFlow.Application.Notifications.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Queries.GetNotificationsByUser;

/// <summary>Query to retrieve a paged list of notifications for a user.</summary>
/// <param name="UserId">The user whose notifications to retrieve.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record GetNotificationsByUserQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<NotificationDto>>>;
