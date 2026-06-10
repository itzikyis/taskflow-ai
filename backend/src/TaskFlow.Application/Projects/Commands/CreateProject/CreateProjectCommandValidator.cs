using FluentValidation;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

/// <summary>Validates <see cref="CreateProjectCommand"/> before the handler runs.</summary>
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");
    }
}
