using FluentValidation;

namespace TaskFlow.Application.CalendarSync.Commands.AddCalendarSubscription;

/// <summary>Validates <see cref="AddCalendarSubscriptionCommand"/>.</summary>
public sealed class AddCalendarSubscriptionCommandValidator : AbstractValidator<AddCalendarSubscriptionCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public AddCalendarSubscriptionCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.ExternalUrl)
            .NotEmpty().WithMessage("External iCal URL is required.")
            .MaximumLength(2000).WithMessage("External iCal URL must not exceed 2000 characters.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("External iCal URL must be a valid absolute HTTP or HTTPS URL.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters.");
    }
}
