using FluentValidation;

namespace TaskFlow.Application.Goals.Commands.CreateGoal;

/// <summary>Validates <see cref="CreateGoalCommand"/>.</summary>
public sealed class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public CreateGoalCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
