using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Automations.Dtos;

/// <summary>Automation rule read model.</summary>
public sealed record AutomationRuleDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    bool IsEnabled,
    string TriggerType,
    string TriggerValue,
    string ActionType,
    string ActionValue,
    DateTime CreatedAt);
