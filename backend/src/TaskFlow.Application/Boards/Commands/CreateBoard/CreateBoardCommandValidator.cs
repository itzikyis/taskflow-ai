using FluentValidation;

namespace TaskFlow.Application.Boards.Commands.CreateBoard;

/// <summary>Validates <see cref="CreateBoardCommand"/>.</summary>
public sealed class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
