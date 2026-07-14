using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for automation rules.</summary>
public interface IAutomationRuleRepository
{
    Task<IReadOnlyList<AutomationRule>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task<AutomationRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AutomationRule rule, CancellationToken ct = default);
    Task DeleteAsync(AutomationRule rule, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
