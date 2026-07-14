using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for project documents.</summary>
public interface IProjectDocumentRepository
{
    Task<IReadOnlyList<ProjectDocument>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task<ProjectDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProjectDocument document, CancellationToken ct = default);
    Task DeleteAsync(ProjectDocument document, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
