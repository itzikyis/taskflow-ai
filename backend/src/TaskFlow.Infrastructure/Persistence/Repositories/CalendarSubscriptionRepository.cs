using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ICalendarSubscriptionRepository"/>.</summary>
internal sealed class CalendarSubscriptionRepository(ApplicationDbContext context)
    : ICalendarSubscriptionRepository
{
    /// <inheritdoc/>
    public async Task<List<CalendarSubscription>> GetByProjectAsync(Guid projectId, CancellationToken ct = default) =>
        await context.CalendarSubscriptions
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.DisplayName)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<CalendarSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.CalendarSubscriptions
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <inheritdoc/>
    public async Task AddAsync(CalendarSubscription sub, CancellationToken ct = default) =>
        await context.CalendarSubscriptions.AddAsync(sub, ct);

    /// <inheritdoc/>
    public void Remove(CalendarSubscription sub) =>
        context.CalendarSubscriptions.Remove(sub);

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
