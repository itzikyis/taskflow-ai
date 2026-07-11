using FluentValidation;

namespace TaskFlow.Application.AI.Queries.SuggestTaskBreakdown;

/// <summary>Validates <see cref="SuggestTaskBreakdownQuery"/> before any AI call is made.</summary>
public sealed class SuggestTaskBreakdownQueryValidator : AbstractValidator<SuggestTaskBreakdownQuery>
{
    public SuggestTaskBreakdownQueryValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
    }
}
