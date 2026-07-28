using FluentValidation;

namespace TaskFlow.Application.TaskTemplates.Commands.CreateTaskFromTemplate;

/// <summary>Validates <see cref="CreateTaskFromTemplateCommand"/> inputs.</summary>
public sealed class CreateTaskFromTemplateCommandValidator : AbstractValidator<CreateTaskFromTemplateCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public CreateTaskFromTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("CreatedByUserId is required.");
    }
}
