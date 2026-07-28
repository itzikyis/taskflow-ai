using FluentValidation;

namespace TaskFlow.Application.Goals.Commands.DeleteGoal;

/// <summary>Validates <see cref="DeleteGoalCommand"/>.</summary>
public sealed class DeleteGoalCommandValidator : AbstractValidator<DeleteGoalCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public DeleteGoalCommandValidator()
    {
        RuleFor(x => x.GoalId).NotEmpty();
    }
}
