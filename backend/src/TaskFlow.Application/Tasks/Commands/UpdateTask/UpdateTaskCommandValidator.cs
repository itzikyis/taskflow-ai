using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.UpdateTask;

/// <summary>Validates <see cref="UpdateTaskCommand"/> inputs before the handler runs.</summary>
public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description != null);
    }
}
