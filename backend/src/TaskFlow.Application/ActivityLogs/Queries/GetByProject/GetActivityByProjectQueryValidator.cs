using FluentValidation;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByProject;

/// <summary>Validates <see cref="GetActivityByProjectQuery"/> pagination parameters.</summary>
public sealed class GetActivityByProjectQueryValidator : AbstractValidator<GetActivityByProjectQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetActivityByProjectQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
