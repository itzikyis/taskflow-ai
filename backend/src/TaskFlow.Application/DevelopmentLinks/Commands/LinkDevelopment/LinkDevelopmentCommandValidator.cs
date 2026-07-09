using FluentValidation;

namespace TaskFlow.Application.DevelopmentLinks.Commands.LinkDevelopment;

/// <summary>Validates <see cref="LinkDevelopmentCommand"/> inputs before the handler runs.</summary>
public sealed class LinkDevelopmentCommandValidator : AbstractValidator<LinkDevelopmentCommand>
{
    public LinkDevelopmentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Repository).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.RefType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}
