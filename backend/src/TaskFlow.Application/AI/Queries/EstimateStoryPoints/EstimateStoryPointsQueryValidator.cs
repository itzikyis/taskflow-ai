using FluentValidation;

namespace TaskFlow.Application.AI.Queries.EstimateStoryPoints;

/// <summary>Validates <see cref="EstimateStoryPointsQuery"/> before any AI call is made.</summary>
public sealed class EstimateStoryPointsQueryValidator : AbstractValidator<EstimateStoryPointsQuery>
{
    public EstimateStoryPointsQueryValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}
