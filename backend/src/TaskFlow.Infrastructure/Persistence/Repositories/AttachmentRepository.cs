using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IAttachmentRepository"/>.</summary>
internal sealed class AttachmentRepository(ApplicationDbContext context) : IAttachmentRepository
{
    public Task<Attachment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Attachments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Attachment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
        await context.Attachments.Where(a => a.TaskId == taskId).ToListAsync(ct);

    public async Task AddAsync(Attachment attachment, CancellationToken ct = default) =>
        await context.Attachments.AddAsync(attachment, ct);

    public void Remove(Attachment attachment) => context.Attachments.Remove(attachment);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
