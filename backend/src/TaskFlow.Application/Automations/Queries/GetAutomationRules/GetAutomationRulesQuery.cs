using MediatR;
using TaskFlow.Application.Automations.Dtos;

namespace TaskFlow.Application.Automations.Queries.GetAutomationRules;

/// <summary>Returns all automation rules for a project.</summary>
public sealed record GetAutomationRulesQuery(Guid ProjectId) : IRequest<IReadOnlyList<AutomationRuleDto>>;
