using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="INotificationRepository"/>.</summary>
internal sealed class NotificationRepository(ApplicationDbContext context) : INotificationRepository
{
    /// <inheritdoc/>
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default) =>
        await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) =>
        context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    /// <inheritdoc/>
    public async Task AddAsync(Notification notification, CancellationToken ct = default) =>
        await context.Notifications.AddAsync(notification, ct);

    /// <inheritdoc/>
    public void Update(Notification notification) =>
        context.Notifications.Update(notification);

    /// <inheritdoc/>
    public void RemoveRange(IEnumerable<Notification> notifications) =>
        context.Notifications.RemoveRange(notifications);

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
