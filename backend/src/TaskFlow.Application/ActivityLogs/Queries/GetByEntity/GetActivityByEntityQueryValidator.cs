using FluentValidation;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByEntity;

/// <summary>Validates <see cref="GetActivityByEntityQuery"/> pagination parameters.</summary>
public sealed class GetActivityByEntityQueryValidator : AbstractValidator<GetActivityByEntityQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetActivityByEntityQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
