using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IActivityLogRepository"/>.</summary>
internal sealed class ActivityLogRepository(ApplicationDbContext context) : IActivityLogRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivityLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await context.ActivityLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivityLog>> GetByProjectAsync(
        Guid projectId,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await context.ActivityLogs
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivityLog>> GetByActorAsync(
        Guid actorId,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await context.ActivityLogs
            .Where(a => a.ActorId == actorId)
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivityLog>> GetRecentAsync(
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await context.ActivityLogs
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(ActivityLog log, CancellationToken ct = default) =>
        await context.ActivityLogs.AddAsync(log, ct);

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
