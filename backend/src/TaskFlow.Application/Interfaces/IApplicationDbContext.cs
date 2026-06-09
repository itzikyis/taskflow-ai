using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Abstraction over EF Core DbContext for use in Application layer tests.</summary>
public interface IApplicationDbContext
{
    DbSet<TaskItem> Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
