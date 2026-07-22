using FluentValidation;

namespace TaskFlow.Application.AI.Queries.AssessSprintRisk;

/// <summary>Validates <see cref="AssessSprintRiskQuery"/> before any AI call is made.</summary>
public sealed class AssessSprintRiskQueryValidator : AbstractValidator<AssessSprintRiskQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public AssessSprintRiskQueryValidator()
    {
        RuleFor(x => x.Tasks)
            .NotEmpty().WithMessage("At least one task is required for risk assessment.");

        RuleForEach(x => x.Tasks).ChildRules(task =>
        {
            task.RuleFor(t => t.Id)
                .NotEmpty().WithMessage("Task id must not be empty (Guid.Empty is not allowed).");

            task.RuleFor(t => t.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(500).WithMessage("Task title must not exceed 500 characters.");
        });
    }
}
