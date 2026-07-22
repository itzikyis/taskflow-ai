using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ICustomFieldRepository"/>.</summary>
internal sealed class CustomFieldRepository(ApplicationDbContext db) : ICustomFieldRepository
{
    /// <inheritdoc/>
    public async Task<List<CustomField>> GetByProjectAsync(Guid projectId, CancellationToken ct) =>
        await db.CustomFields
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<CustomField?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.CustomFields.FindAsync([id], ct);

    /// <inheritdoc/>
    public async Task AddAsync(CustomField field, CancellationToken ct) =>
        await db.CustomFields.AddAsync(field, ct);

    /// <inheritdoc/>
    public Task RemoveAsync(CustomField field, CancellationToken ct)
    {
        db.CustomFields.Remove(field);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<List<CustomFieldValue>> GetValuesByTaskAsync(Guid taskId, CancellationToken ct) =>
        await db.CustomFieldValues
            .AsNoTracking()
            .Where(v => v.TaskId == taskId)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task UpsertValueAsync(CustomFieldValue value, CancellationToken ct)
    {
        var existing = await db.CustomFieldValues
            .FirstOrDefaultAsync(v => v.TaskId == value.TaskId && v.CustomFieldId == value.CustomFieldId, ct);

        if (existing is null)
            await db.CustomFieldValues.AddAsync(value, ct);
        else
            existing.SetValue(value.Value);
    }

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
