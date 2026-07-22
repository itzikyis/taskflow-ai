using FluentValidation;

namespace TaskFlow.Application.CustomFields.Commands.SetCustomFieldValue;

/// <summary>Validates <see cref="SetCustomFieldValueCommand"/> before it reaches the handler.</summary>
public sealed class SetCustomFieldValueCommandValidator : AbstractValidator<SetCustomFieldValueCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public SetCustomFieldValueCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.CustomFieldId)
            .NotEmpty().WithMessage("Custom field ID is required.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value must not be null.")
            .MaximumLength(1000).WithMessage("Value must not exceed 1000 characters.");
    }
}
