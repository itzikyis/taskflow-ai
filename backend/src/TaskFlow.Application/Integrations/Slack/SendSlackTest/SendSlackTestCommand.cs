using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Integrations.Slack.SendSlackTest;

/// <summary>Sends a test message through the configured Slack webhook.</summary>
public sealed record SendSlackTestCommand : IRequest<Result>;

/// <summary>Handles <see cref="SendSlackTestCommand"/>.</summary>
public sealed class SendSlackTestCommandHandler(
    ISlackIntegrationRepository repo,
    IExternalNotificationService notifier)
    : IRequestHandler<SendSlackTestCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(SendSlackTestCommand request, CancellationToken ct)
    {
        var slack = await repo.GetAsync(ct);
        if (slack is null || string.IsNullOrWhiteSpace(slack.WebhookUrl))
            return Result.Failure(new Error("Slack.NotConfigured", "No Slack webhook is configured."));

        try
        {
            await notifier.SendAsync(slack.WebhookUrl, ":taskflow: ✅ TaskFlow AI is connected to this channel.", ct);
            return Result.Ok;
        }
        catch (Exception)
        {
            return Result.Failure(new Error("Slack.DeliveryFailed", "Could not deliver the test message to Slack."));
        }
    }
}
