using FluentValidation;

namespace TaskFlow.Application.AI.Queries.NaturalLanguageSearch;

/// <summary>Validates <see cref="NaturalLanguageSearchQuery"/> before it reaches the handler.</summary>
public sealed class NaturalLanguageSearchQueryValidator : AbstractValidator<NaturalLanguageSearchQuery>
{
    /// <summary>Initialises the validation rules.</summary>
    public NaturalLanguageSearchQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search query must not be empty.")
            .MaximumLength(500).WithMessage("Search query must not exceed 500 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
