using FluentValidation;

namespace TaskFlow.Application.Comments.Commands.EditComment;

/// <summary>Validates <see cref="EditCommentCommand"/>.</summary>
public sealed class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public EditCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.RequesterId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
    }
}
