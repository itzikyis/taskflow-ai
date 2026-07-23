namespace TaskFlow.Application.Integrations.Slack.GetSlackCommandConfig;

/// <summary>DTO describing the current Slack slash-command integration status.</summary>
public sealed record SlackCommandConfigDto(
    /// <summary>Whether <c>Slack:SigningSecret</c> is set in backend configuration.</summary>
    bool IsConfigured);
