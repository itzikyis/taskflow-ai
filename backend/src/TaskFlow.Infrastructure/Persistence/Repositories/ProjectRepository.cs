using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IProjectRepository"/>.</summary>
internal sealed class ProjectRepository(ApplicationDbContext context) : IProjectRepository
{
    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Projects.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Project>> GetAllAsync(
        Guid? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Project> query = context.Projects.AsNoTracking();

        if (ownerId.HasValue)
            query = query.Where(p => p.OwnerId == ownerId.Value);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        await context.Projects.AddAsync(project, cancellationToken);

    public void Update(Project project) =>
        context.Projects.Update(project);

    public void Remove(Project project) =>
        context.Projects.Remove(project);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
