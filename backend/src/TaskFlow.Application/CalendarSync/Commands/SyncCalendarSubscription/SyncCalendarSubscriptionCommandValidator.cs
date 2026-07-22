using FluentValidation;

namespace TaskFlow.Application.CalendarSync.Commands.SyncCalendarSubscription;

/// <summary>Validates <see cref="SyncCalendarSubscriptionCommand"/>.</summary>
public sealed class SyncCalendarSubscriptionCommandValidator : AbstractValidator<SyncCalendarSubscriptionCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public SyncCalendarSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created-by user ID is required.");
    }
}
