using FluentValidation;

namespace TaskFlow.Application.TaskTemplates.Commands.DeleteTaskTemplate;

/// <summary>Validates <see cref="DeleteTaskTemplateCommand"/> inputs.</summary>
public sealed class DeleteTaskTemplateCommandValidator : AbstractValidator<DeleteTaskTemplateCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public DeleteTaskTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required.");
    }
}
