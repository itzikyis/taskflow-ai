using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Evaluates automation rules after a task event and fires the matching actions.</summary>
public interface IAutomationEvaluatorService
{
    /// <summary>Evaluates all enabled rules for the task's project and applies matching actions.</summary>
    Task EvaluateAsync(
        TaskItem task,
        AutomationTriggerType trigger,
        string triggerValue,
        CancellationToken ct = default);
}
