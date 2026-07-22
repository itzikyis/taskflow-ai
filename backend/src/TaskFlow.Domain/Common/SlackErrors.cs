namespace TaskFlow.Domain.Common;

/// <summary>Domain errors related to Slack integration operations.</summary>
public static class SlackErrors
{
    /// <summary>Raised when the task title is missing from the Slack slash command.</summary>
    public static readonly Error TitleRequired =
        new("Slack.TitleRequired", "Please provide a task title. Usage: /taskflow create <title>");

    /// <summary>Raised when the Slack request signature verification fails.</summary>
    public static readonly Error InvalidSignature =
        new("Slack.InvalidSignature", "Slack request signature verification failed.");

    /// <summary>Raised when no system user is configured to own Slack-created tasks.</summary>
    public static readonly Error SystemUserNotConfigured =
        new("Slack.SystemUserNotConfigured", "No system user is configured for Slack task creation.");
}
