using FluentValidation;

namespace TaskFlow.Application.Integrations.Slack.SaveSlackConfig;

/// <summary>Validates <see cref="SaveSlackConfigCommand"/>.</summary>
public sealed class SaveSlackConfigCommandValidator : AbstractValidator<SaveSlackConfigCommand>
{
    public SaveSlackConfigCommandValidator()
    {
        RuleFor(x => x.WebhookUrl)
            .NotEmpty().WithMessage("Webhook URL is required.")
            .Must(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Webhook URL must be an https URL.")
            .MaximumLength(1024);
    }
}
