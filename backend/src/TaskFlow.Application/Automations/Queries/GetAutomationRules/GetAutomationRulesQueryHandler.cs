using MediatR;
using TaskFlow.Application.Automations.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Automations.Queries.GetAutomationRules;

/// <summary>Handles <see cref="GetAutomationRulesQuery"/>.</summary>
public sealed class GetAutomationRulesQueryHandler(IAutomationRuleRepository repo)
    : IRequestHandler<GetAutomationRulesQuery, IReadOnlyList<AutomationRuleDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AutomationRuleDto>> Handle(GetAutomationRulesQuery request, CancellationToken ct)
    {
        var rules = await repo.GetByProjectAsync(request.ProjectId, ct);
        return rules.Select(r => new AutomationRuleDto(
            r.Id, r.ProjectId, r.Name, r.IsEnabled,
            r.TriggerType.ToString(), r.TriggerValue,
            r.ActionType.ToString(), r.ActionValue,
            r.CreatedAt)).ToList();
    }
}
