using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="Attachment"/> aggregates.</summary>
public interface IAttachmentRepository
{
    /// <summary>Returns an attachment by id, or null if not found.</summary>
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all attachments for a given task.</summary>
    Task<IReadOnlyList<Attachment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>Adds a new attachment to the context.</summary>
    Task AddAsync(Attachment attachment, CancellationToken ct = default);

    /// <summary>Marks an attachment for deletion.</summary>
    void Remove(Attachment attachment);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
