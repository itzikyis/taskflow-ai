using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>Trigger that fires an automation rule.</summary>
public enum AutomationTriggerType
{
    TaskStatusChanged = 0,
    TaskCreated = 1,
    TaskPriorityChanged = 2,
}

/// <summary>Action performed when an automation rule fires.</summary>
public enum AutomationActionType
{
    SendNotification = 0,
    PostComment = 1,
    ChangeStatus = 2,
}

/// <summary>
/// A no-code if/then automation rule scoped to a project.
/// When the trigger condition matches a task event, the configured action is applied.
/// </summary>
public sealed class AutomationRule : AggregateRoot
{
    private AutomationRule() { } // EF Core

    /// <summary>The project this rule belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Human-readable name for this rule.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Whether the rule is currently active.</summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>What event triggers this rule.</summary>
    public AutomationTriggerType TriggerType { get; private set; }

    /// <summary>
    /// The value to match for the trigger.
    /// For StatusChanged: the new status string (e.g. "Done").
    /// For PriorityChanged: the new priority string (e.g. "Critical").
    /// For TaskCreated: empty (matches all).
    /// </summary>
    public string TriggerValue { get; private set; } = string.Empty;

    /// <summary>The action to perform when the rule fires.</summary>
    public AutomationActionType ActionType { get; private set; }

    /// <summary>
    /// The value for the action.
    /// For SendNotification / PostComment: the message text.
    /// For ChangeStatus: the target status string.
    /// </summary>
    public string ActionValue { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    /// <summary>Creates a new automation rule.</summary>
    public static AutomationRule Create(
        Guid projectId,
        string name,
        AutomationTriggerType triggerType,
        string triggerValue,
        AutomationActionType actionType,
        string actionValue)
    {
        return new AutomationRule
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name.Trim(),
            IsEnabled = true,
            TriggerType = triggerType,
            TriggerValue = triggerValue.Trim(),
            ActionType = actionType,
            ActionValue = actionValue.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>Toggles the enabled state of the rule.</summary>
    public void SetEnabled(bool enabled) => IsEnabled = enabled;

    /// <summary>Updates the rule's mutable fields.</summary>
    public void Update(string name, string triggerValue, string actionValue)
    {
        Name = name.Trim();
        TriggerValue = triggerValue.Trim();
        ActionValue = actionValue.Trim();
    }
}
