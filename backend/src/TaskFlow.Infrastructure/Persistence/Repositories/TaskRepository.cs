using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITaskRepository"/>.</summary>
internal sealed class TaskRepository(ApplicationDbContext context) : ITaskRepository
{
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Tasks.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(
        Guid? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskItem> query = context.Tasks.AsNoTracking();

        if (assignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await context.Tasks.AddAsync(task, cancellationToken);

    public void Update(TaskItem task) =>
        context.Tasks.Update(task);

    public void Remove(TaskItem task) =>
        context.Tasks.Remove(task);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
