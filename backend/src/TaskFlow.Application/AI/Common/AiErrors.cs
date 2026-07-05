using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Common;

/// <summary>Well-known errors returned by AI-assisted feature handlers.</summary>
public static class AiErrors
{
    /// <summary>
    /// A transient failure talking to the AI provider (network error, malformed
    /// response, timeout). Typically safe to retry.
    /// </summary>
    public static readonly Error Unavailable =
        new("AI.Unavailable", "AI service is temporarily unavailable. Please try again.");

    /// <summary>
    /// The AI provider is not configured (e.g. missing API key). This is a
    /// deployment/configuration problem, not a transient outage — retrying will
    /// not help until an operator fixes the configuration.
    /// </summary>
    public static readonly Error NotConfigured =
        new("AI.NotConfigured", "AI service is not configured. Contact an administrator.");
}
