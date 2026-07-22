using FluentValidation;

namespace TaskFlow.Application.CustomFields.Commands.DeleteCustomField;

/// <summary>Validates <see cref="DeleteCustomFieldCommand"/> before it reaches the handler.</summary>
public sealed class DeleteCustomFieldCommandValidator : AbstractValidator<DeleteCustomFieldCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public DeleteCustomFieldCommandValidator()
    {
        RuleFor(x => x.FieldId)
            .NotEmpty().WithMessage("Field ID is required.");
    }
}
