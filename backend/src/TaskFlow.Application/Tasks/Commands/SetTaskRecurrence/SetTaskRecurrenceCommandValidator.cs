using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.SetTaskRecurrence;

/// <summary>Validates <see cref="SetTaskRecurrenceCommand"/> inputs before the handler runs.</summary>
public sealed class SetTaskRecurrenceCommandValidator : AbstractValidator<SetTaskRecurrenceCommand>
{
    private static readonly string[] ValidPatterns = ["daily", "weekly", "monthly"];

    /// <summary>Initialises validation rules.</summary>
    public SetTaskRecurrenceCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");

        RuleFor(x => x.Pattern)
            .NotEmpty().WithMessage("Recurrence pattern is required.")
            .Must(p => ValidPatterns.Contains(p))
            .WithMessage("Pattern must be 'daily', 'weekly', or 'monthly'.");
    }
}
