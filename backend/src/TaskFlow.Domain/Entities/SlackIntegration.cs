using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Workspace-level Slack Incoming Webhook configuration. Task activity is
/// forwarded to <see cref="WebhookUrl"/> based on the per-event-type flags.
/// </summary>
public sealed class SlackIntegration : AggregateRoot
{
    private SlackIntegration() { }

    private SlackIntegration(Guid id, string webhookUrl)
    {
        Id = id;
        WebhookUrl = webhookUrl;
        Enabled = true;
        ForwardCreated = true;
        ForwardStatusChanged = true;
        ForwardComments = true;
        ForwardOther = false;
        CreatedAt = DateTime.UtcNow;
    }

    public string WebhookUrl { get; private set; } = string.Empty;
    public bool Enabled { get; private set; }
    public bool ForwardCreated { get; private set; }
    public bool ForwardStatusChanged { get; private set; }
    public bool ForwardComments { get; private set; }
    public bool ForwardOther { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Creates a new Slack integration for the given Incoming Webhook URL.</summary>
    public static SlackIntegration Create(string webhookUrl) => new(Guid.NewGuid(), webhookUrl.Trim());

    /// <summary>Updates the webhook URL and per-event forwarding flags.</summary>
    public void Update(string webhookUrl, bool enabled, bool created, bool statusChanged, bool comments, bool other)
    {
        WebhookUrl = webhookUrl.Trim();
        Enabled = enabled;
        ForwardCreated = created;
        ForwardStatusChanged = statusChanged;
        ForwardComments = comments;
        ForwardOther = other;
        UpdatedAt = DateTime.UtcNow;
    }
}
