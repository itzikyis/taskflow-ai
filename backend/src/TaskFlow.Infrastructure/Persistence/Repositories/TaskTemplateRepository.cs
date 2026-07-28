using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITaskTemplateRepository"/>.</summary>
internal sealed class TaskTemplateRepository(ApplicationDbContext context) : ITaskTemplateRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskTemplate>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await context.TaskTemplates
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<TaskTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.TaskTemplates.FindAsync([id], cancellationToken);

    /// <inheritdoc/>
    public async Task AddAsync(TaskTemplate template, CancellationToken cancellationToken = default) =>
        await context.TaskTemplates.AddAsync(template, cancellationToken);

    /// <inheritdoc/>
    public void Delete(TaskTemplate template) =>
        context.TaskTemplates.Remove(template);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
