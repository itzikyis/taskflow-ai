using FluentValidation;

namespace TaskFlow.Application.ActivityLogs.Queries.GetRecent;

/// <summary>Validates <see cref="GetRecentActivityQuery"/> pagination parameters.</summary>
public sealed class GetRecentActivityQueryValidator : AbstractValidator<GetRecentActivityQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetRecentActivityQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
