using FluentValidation;

namespace TaskFlow.Application.CalendarSync.Queries.GetCalendarSubscriptions;

/// <summary>Validates <see cref="GetCalendarSubscriptionsQuery"/>.</summary>
public sealed class GetCalendarSubscriptionsQueryValidator : AbstractValidator<GetCalendarSubscriptionsQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetCalendarSubscriptionsQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
