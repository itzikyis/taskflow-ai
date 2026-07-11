using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.CreateSubtasks;

/// <summary>Validates <see cref="CreateSubtasksCommand"/>.</summary>
public sealed class CreateSubtasksCommandValidator : AbstractValidator<CreateSubtasksCommand>
{
    public CreateSubtasksCommandValidator()
    {
        RuleFor(x => x.ParentTaskId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.Subtasks).NotEmpty().WithMessage("At least one subtask is required.");
        RuleForEach(x => x.Subtasks).ChildRules(s =>
        {
            s.RuleFor(t => t.Title)
                .NotEmpty().WithMessage("Subtask title is required.")
                .MaximumLength(200).WithMessage("Subtask title must not exceed 200 characters.");
        });
    }
}
