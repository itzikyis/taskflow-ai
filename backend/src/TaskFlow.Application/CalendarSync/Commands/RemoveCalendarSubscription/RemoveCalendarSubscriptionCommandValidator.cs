using FluentValidation;

namespace TaskFlow.Application.CalendarSync.Commands.RemoveCalendarSubscription;

/// <summary>Validates <see cref="RemoveCalendarSubscriptionCommand"/>.</summary>
public sealed class RemoveCalendarSubscriptionCommandValidator : AbstractValidator<RemoveCalendarSubscriptionCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public RemoveCalendarSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
