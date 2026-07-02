using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IAuditRepository"/>. Append-only — no updates or deletes.</summary>
internal sealed class AuditRepository(ApplicationDbContext context) : IAuditRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(AuditEntry entry, CancellationToken ct = default) =>
        await context.AuditEntries.AddAsync(entry, ct);

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditEntry>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default) =>
        await context.AuditEntries
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.OccurredAt)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditEntry>> GetByActorAsync(
        Guid actorId,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await context.AuditEntries
            .Where(a => a.ActorId == actorId)
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditEntry>> GetRecentAsync(
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await context.AuditEntries
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
}
