using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITaskDependencyRepository"/>.</summary>
internal sealed class TaskDependencyRepository(ApplicationDbContext context) : ITaskDependencyRepository
{
    public Task<TaskDependency?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.TaskDependencies.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<TaskDependency>> GetAllAsync(CancellationToken ct = default) =>
        await context.TaskDependencies.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<TaskDependency>> GetByTaskAsync(Guid taskId, CancellationToken ct = default) =>
        await context.TaskDependencies
            .Where(d => d.TaskId == taskId || d.BlockedByTaskId == taskId)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid taskId, Guid blockedByTaskId, CancellationToken ct = default) =>
        context.TaskDependencies.AnyAsync(d => d.TaskId == taskId && d.BlockedByTaskId == blockedByTaskId, ct);

    public async Task AddAsync(TaskDependency dependency, CancellationToken ct = default) =>
        await context.TaskDependencies.AddAsync(dependency, ct);

    public void Remove(TaskDependency dependency) => context.TaskDependencies.Remove(dependency);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
