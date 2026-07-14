using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Automations.Commands.ToggleAutomationRule;

/// <summary>Enables or disables an automation rule.</summary>
public sealed record ToggleAutomationRuleCommand(Guid RuleId, bool IsEnabled) : IRequest<Result>;
