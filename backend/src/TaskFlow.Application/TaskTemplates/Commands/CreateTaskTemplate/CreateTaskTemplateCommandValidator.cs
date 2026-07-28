using FluentValidation;

namespace TaskFlow.Application.TaskTemplates.Commands.CreateTaskTemplate;

/// <summary>Validates <see cref="CreateTaskTemplateCommand"/> inputs before the handler runs.</summary>
public sealed class CreateTaskTemplateCommandValidator : AbstractValidator<CreateTaskTemplateCommand>
{
    private static readonly string[] ValidPriorities = ["Low", "Medium", "High", "Critical"];

    /// <summary>Initialises validation rules.</summary>
    public CreateTaskTemplateCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters.");

        RuleFor(x => x.DefaultTitle)
            .NotEmpty().WithMessage("Default title is required.")
            .MaximumLength(200).WithMessage("Default title must not exceed 200 characters.");

        RuleFor(x => x.DefaultPriority)
            .Must(p => p is null || ValidPriorities.Contains(p))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");

        RuleFor(x => x.DefaultEstimatedHours)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DefaultEstimatedHours.HasValue)
            .WithMessage("Estimated hours must not be negative.");
    }
}
