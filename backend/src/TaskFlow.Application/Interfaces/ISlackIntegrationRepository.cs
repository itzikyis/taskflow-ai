using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for the single workspace-level <see cref="SlackIntegration"/>.</summary>
public interface ISlackIntegrationRepository
{
    /// <summary>Returns the configured Slack integration, or null if none is set up.</summary>
    Task<SlackIntegration?> GetAsync(CancellationToken ct = default);

    /// <summary>Adds a new integration.</summary>
    Task AddAsync(SlackIntegration integration, CancellationToken ct = default);

    /// <summary>Marks the integration as modified.</summary>
    void Update(SlackIntegration integration);

    /// <summary>Removes the integration.</summary>
    void Remove(SlackIntegration integration);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
