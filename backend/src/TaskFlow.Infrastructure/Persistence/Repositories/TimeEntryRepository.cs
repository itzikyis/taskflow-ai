using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITimeEntryRepository"/>.</summary>
internal sealed class TimeEntryRepository(ApplicationDbContext context) : ITimeEntryRepository
{
    public Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.TimeEntries.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<TimeEntry>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
        await context.TimeEntries
            .Where(e => e.TaskId == taskId)
            .OrderByDescending(e => e.LoggedAt)
            .ToListAsync(ct);

    public async Task<int> GetTotalMinutesByTaskAsync(Guid taskId, CancellationToken ct = default) =>
        await context.TimeEntries.Where(e => e.TaskId == taskId).SumAsync(e => e.Minutes, ct);

    public async Task AddAsync(TimeEntry entry, CancellationToken ct = default) =>
        await context.TimeEntries.AddAsync(entry, ct);

    public void Remove(TimeEntry entry) => context.TimeEntries.Remove(entry);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
