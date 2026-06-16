using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IBoardRepository"/>.</summary>
internal sealed class BoardRepository(ApplicationDbContext context) : IBoardRepository
{
    public Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Boards.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Board>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default) =>
        await context.Boards
                     .Where(b => b.ProjectId == projectId)
                     .OrderByDescending(b => b.CreatedAt)
                     .ToListAsync(ct);

    public async Task AddAsync(Board board, CancellationToken ct = default) =>
        await context.Boards.AddAsync(board, ct);

    public void Update(Board board) => context.Boards.Update(board);
    public void Remove(Board board) => context.Boards.Remove(board);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
