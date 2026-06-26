using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Notifications.Commands.DeleteAllRead;
using TaskFlow.Application.Notifications.Commands.DeleteNotification;
using TaskFlow.Application.Notifications.Commands.MarkAllAsRead;
using TaskFlow.Application.Notifications.Commands.MarkAsRead;
using TaskFlow.Application.Notifications.Dtos;
using TaskFlow.Application.Notifications.Queries.GetNotificationsByUser;
using TaskFlow.Application.Notifications.Queries.GetUnreadCount;
using TaskFlow.Domain.Common;

namespace TaskFlow.API.Controllers;

/// <summary>Endpoints for managing user notifications.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns a paged list of notifications for the specified user.</summary>
    /// <param name="userId">Recipient user ID.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(
        [FromQuery] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetNotificationsByUserQuery(userId, page, pageSize),
            cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>Returns the number of unread notifications for the specified user.</summary>
    /// <param name="userId">Recipient user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetUnreadCountQuery(userId), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Marks a single notification as read.</summary>
    /// <param name="id">Notification ID.</param>
    /// <param name="userId">User ID of the requester (must own the notification).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new MarkAsReadCommand(id, userId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == NotificationErrors.NotFound.Code)
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>Marks all notifications for a user as read.</summary>
    /// <param name="userId">The user whose notifications should be marked read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new MarkAllAsReadCommand(userId), cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a single notification.</summary>
    /// <param name="id">Notification ID.</param>
    /// <param name="userId">User ID of the requester (must own the notification).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteNotificationCommand(id, userId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == NotificationErrors.NotFound.Code)
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>Deletes all read notifications for a user.</summary>
    /// <param name="userId">The user whose read notifications should be deleted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAllRead(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteAllReadCommand(userId), cancellationToken);
        return NoContent();
    }
}
