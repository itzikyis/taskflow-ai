using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IProjectDocumentRepository"/>.</summary>
internal sealed class ProjectDocumentRepository(ApplicationDbContext db) : IProjectDocumentRepository
{
    public async Task<IReadOnlyList<ProjectDocument>> GetByProjectAsync(Guid projectId, CancellationToken ct) =>
        await db.ProjectDocuments
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(ct);

    public async Task<ProjectDocument?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.ProjectDocuments.FindAsync([id], ct);

    public async Task AddAsync(ProjectDocument document, CancellationToken ct) =>
        await db.ProjectDocuments.AddAsync(document, ct);

    public Task DeleteAsync(ProjectDocument document, CancellationToken ct)
    {
        db.ProjectDocuments.Remove(document);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
