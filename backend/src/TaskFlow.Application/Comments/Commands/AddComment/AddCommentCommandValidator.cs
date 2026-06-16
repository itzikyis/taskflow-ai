using FluentValidation;

namespace TaskFlow.Application.Comments.Commands.AddComment;

/// <summary>Validates <see cref="AddCommentCommand"/>.</summary>
public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.AuthorId).NotEmpty();
    }
}
