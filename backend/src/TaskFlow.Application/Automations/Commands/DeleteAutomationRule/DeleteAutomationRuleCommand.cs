using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Automations.Commands.DeleteAutomationRule;

/// <summary>Deletes an automation rule by id.</summary>
public sealed record DeleteAutomationRuleCommand(Guid RuleId) : IRequest<Result>;
