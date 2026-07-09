using TaskFlow.Application.DevelopmentLinks.GitHub;

namespace TaskFlow.Application.Interfaces;

/// <summary>Parses raw GitHub webhook payloads into normalised development references.</summary>
public interface IGitHubWebhookParser
{
    /// <summary>
    /// Parses a GitHub webhook payload for the given event type ("push" or
    /// "pull_request") into zero or more development references. Unsupported
    /// event types yield an empty list.
    /// </summary>
    /// <param name="eventType">The value of the <c>X-GitHub-Event</c> header.</param>
    /// <param name="jsonPayload">The raw JSON request body.</param>
    IReadOnlyList<ParsedDevelopmentRef> Parse(string eventType, string jsonPayload);
}
