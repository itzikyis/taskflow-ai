using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ICommentRepository"/>.</summary>
internal sealed class CommentRepository(ApplicationDbContext context) : ICommentRepository
{
    /// <inheritdoc/>
    public Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Comments.FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Comment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
        await context.Comments.AsNoTracking().Where(c => c.TaskId == taskId).ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(Comment comment, CancellationToken ct = default) =>
        await context.Comments.AddAsync(comment, ct);

    /// <inheritdoc/>
    public void Update(Comment comment) => context.Comments.Update(comment);

    /// <inheritdoc/>
    public void Remove(Comment comment) => context.Comments.Remove(comment);

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
