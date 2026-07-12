using FluentValidation;

namespace TaskFlow.Application.TimeTracking.Commands.LogTime;

/// <summary>Validates <see cref="LogTimeCommand"/>.</summary>
public sealed class LogTimeCommandValidator : AbstractValidator<LogTimeCommand>
{
    public LogTimeCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Minutes).GreaterThan(0).LessThanOrEqualTo(24 * 60);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
