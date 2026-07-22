using FluentValidation;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByActor;

/// <summary>Validates <see cref="GetActivityByActorQuery"/> pagination parameters.</summary>
public sealed class GetActivityByActorQueryValidator : AbstractValidator<GetActivityByActorQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetActivityByActorQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
