using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="User"/> aggregates.</summary>
public interface IUserRepository
{
    /// <summary>Returns the user with the given id, or null.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns the user with the given email (case-insensitive), or null.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Returns true when an account with that email already exists.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Adds a new user to the store.</summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Persists pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
