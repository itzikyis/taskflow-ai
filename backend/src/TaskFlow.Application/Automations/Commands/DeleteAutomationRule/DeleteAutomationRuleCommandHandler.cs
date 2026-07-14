using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Automations.Commands.DeleteAutomationRule;

/// <summary>Handles <see cref="DeleteAutomationRuleCommand"/>.</summary>
public sealed class DeleteAutomationRuleCommandHandler(IAutomationRuleRepository repo)
    : IRequestHandler<DeleteAutomationRuleCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteAutomationRuleCommand request, CancellationToken ct)
    {
        var rule = await repo.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result.Failure(new Error("Automation.NotFound", $"Rule {request.RuleId} not found."));

        await repo.DeleteAsync(rule, ct);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
