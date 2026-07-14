using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Automations.Commands.CreateAutomationRule;

/// <summary>Handles <see cref="CreateAutomationRuleCommand"/>.</summary>
public sealed class CreateAutomationRuleCommandHandler(IAutomationRuleRepository repo)
    : IRequestHandler<CreateAutomationRuleCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(CreateAutomationRuleCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure(new Error("Automation.InvalidName", "Rule name cannot be empty."));
        if (string.IsNullOrWhiteSpace(request.ActionValue))
            return Result<Guid>.Failure(new Error("Automation.InvalidAction", "Action value cannot be empty."));

        var rule = AutomationRule.Create(
            request.ProjectId, request.Name,
            request.TriggerType, request.TriggerValue,
            request.ActionType, request.ActionValue);

        await repo.AddAsync(rule, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(rule.Id);
    }
}
