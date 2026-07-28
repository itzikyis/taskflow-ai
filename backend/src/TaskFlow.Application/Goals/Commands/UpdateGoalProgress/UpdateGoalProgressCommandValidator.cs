using FluentValidation;

namespace TaskFlow.Application.Goals.Commands.UpdateGoalProgress;

/// <summary>Validates <see cref="UpdateGoalProgressCommand"/>.</summary>
public sealed class UpdateGoalProgressCommandValidator : AbstractValidator<UpdateGoalProgressCommand>
{
    private static readonly string[] ValidStatuses = ["OnTrack", "AtRisk", "OffTrack", "Completed"];

    /// <summary>Initialises validation rules.</summary>
    public UpdateGoalProgressCommandValidator()
    {
        RuleFor(x => x.GoalId).NotEmpty();
        RuleFor(x => x.ProgressPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.Status).Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: OnTrack, AtRisk, OffTrack, Completed.");
    }
}
