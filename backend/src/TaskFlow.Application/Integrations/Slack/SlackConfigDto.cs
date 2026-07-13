namespace TaskFlow.Application.Integrations.Slack;

/// <summary>DTO describing the current Slack integration (webhook URL is masked).</summary>
public sealed record SlackConfigDto(
    bool Configured,
    bool Enabled,
    string WebhookUrlMasked,
    bool ForwardCreated,
    bool ForwardStatusChanged,
    bool ForwardComments,
    bool ForwardOther);
