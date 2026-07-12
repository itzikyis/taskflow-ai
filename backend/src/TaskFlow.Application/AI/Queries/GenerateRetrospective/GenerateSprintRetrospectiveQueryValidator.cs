using FluentValidation;

namespace TaskFlow.Application.AI.Queries.GenerateRetrospective;

/// <summary>Validates <see cref="GenerateSprintRetrospectiveQuery"/> before any AI call.</summary>
public sealed class GenerateSprintRetrospectiveQueryValidator
    : AbstractValidator<GenerateSprintRetrospectiveQuery>
{
    public GenerateSprintRetrospectiveQueryValidator()
    {
        RuleFor(x => x.Completed)
            .NotEmpty().WithMessage("At least one completed task is required to generate a retrospective.");
    }
}
