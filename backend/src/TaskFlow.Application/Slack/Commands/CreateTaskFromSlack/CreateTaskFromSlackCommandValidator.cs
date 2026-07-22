using FluentValidation;

namespace TaskFlow.Application.Slack.Commands.CreateTaskFromSlack;

/// <summary>Validates <see cref="CreateTaskFromSlackCommand"/> inputs before the handler runs.</summary>
public sealed class CreateTaskFromSlackCommandValidator : AbstractValidator<CreateTaskFromSlackCommand>
{
    /// <summary>Initialises the validation rules.</summary>
    public CreateTaskFromSlackCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.SlackUserId)
            .NotEmpty().WithMessage("SlackUserId is required.");

        RuleFor(x => x.SlackUserName)
            .NotEmpty().WithMessage("SlackUserName is required.");

        RuleFor(x => x.ChannelId)
            .NotEmpty().WithMessage("ChannelId is required.");
    }
}
