using FluentValidation;

namespace TaskFlow.Application.CustomFields.Commands.CreateCustomField;

/// <summary>Validates <see cref="CreateCustomFieldCommand"/> before it reaches the handler.</summary>
public sealed class CreateCustomFieldCommandValidator : AbstractValidator<CreateCustomFieldCommand>
{
    private static readonly string[] ValidFieldTypes = ["Text", "Number", "Select", "Date"];

    /// <summary>Initialises validation rules.</summary>
    public CreateCustomFieldCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Field name is required.")
            .MaximumLength(100).WithMessage("Field name must not exceed 100 characters.");

        RuleFor(x => x.FieldType)
            .NotEmpty().WithMessage("Field type is required.")
            .Must(t => ValidFieldTypes.Contains(t))
            .WithMessage("Field type must be one of: Text, Number, Select, Date.");
    }
}
