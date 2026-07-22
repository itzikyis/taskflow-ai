using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for custom fields and their task values.</summary>
public interface ICustomFieldRepository
{
    /// <summary>Returns all custom fields defined for the given project.</summary>
    Task<List<CustomField>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Returns a custom field by its ID, or <see langword="null"/> if not found.</summary>
    Task<CustomField?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Adds a new custom field to the store.</summary>
    Task AddAsync(CustomField field, CancellationToken ct = default);

    /// <summary>Removes a custom field from the store.</summary>
    Task RemoveAsync(CustomField field, CancellationToken ct = default);

    /// <summary>Returns all custom field values set on the given task.</summary>
    Task<List<CustomFieldValue>> GetValuesByTaskAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>
    /// Inserts or updates the value record for the (taskId, customFieldId) pair.
    /// </summary>
    Task UpsertValueAsync(CustomFieldValue value, CancellationToken ct = default);

    /// <summary>Persists pending changes to the underlying store.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
