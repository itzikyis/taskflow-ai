using FluentValidation;

namespace TaskFlow.Application.Notifications.Queries.GetNotificationsByUser;

/// <summary>Validates <see cref="GetNotificationsByUserQuery"/> pagination parameters.</summary>
public sealed class GetNotificationsByUserQueryValidator : AbstractValidator<GetNotificationsByUserQuery>
{
    /// <summary>Initialises validation rules.</summary>
    public GetNotificationsByUserQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
