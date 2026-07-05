using FluentValidation;

namespace TaskFlow.Application.AI.Queries.SuggestSprintPlan;

/// <summary>Validates <see cref="SuggestSprintPlanQuery"/> before any AI call is made.</summary>
public sealed class SuggestSprintPlanQueryValidator : AbstractValidator<SuggestSprintPlanQuery>
{
    public SuggestSprintPlanQueryValidator()
    {
        RuleFor(x => x.Backlog)
            .NotEmpty().WithMessage("Backlog must contain at least one task.");

        RuleFor(x => x.SprintCapacity)
            .GreaterThan(0).WithMessage("Sprint capacity must be greater than zero.")
            .LessThanOrEqualTo(500).WithMessage("Sprint capacity must not exceed 500 points.");
    }
}
