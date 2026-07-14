using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Automations.Commands.CreateAutomationRule;

/// <summary>Creates a new automation rule for a project.</summary>
public sealed record CreateAutomationRuleCommand(
    Guid ProjectId,
    string Name,
    AutomationTriggerType TriggerType,
    string TriggerValue,
    AutomationActionType ActionType,
    string ActionValue) : IRequest<Result<Guid>>;
