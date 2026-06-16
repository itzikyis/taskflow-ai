using FluentValidation;

namespace TaskFlow.Application.Boards.Commands.AddColumn;

/// <summary>Validates <see cref="AddColumnCommand"/>.</summary>
public sealed class AddColumnCommandValidator : AbstractValidator<AddColumnCommand>
{
    public AddColumnCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}
