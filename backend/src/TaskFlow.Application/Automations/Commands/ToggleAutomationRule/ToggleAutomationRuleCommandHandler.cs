using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Automations.Commands.ToggleAutomationRule;

/// <summary>Handles <see cref="ToggleAutomationRuleCommand"/>.</summary>
public sealed class ToggleAutomationRuleCommandHandler(IAutomationRuleRepository repo)
    : IRequestHandler<ToggleAutomationRuleCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(ToggleAutomationRuleCommand request, CancellationToken ct)
    {
        var rule = await repo.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result.Failure(new Error("Automation.NotFound", $"Rule {request.RuleId} not found."));

        rule.SetEnabled(request.IsEnabled);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
