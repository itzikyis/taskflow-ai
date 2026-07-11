using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.DevelopmentLinks.GitHub;

/// <summary>
/// A source-control reference extracted from a GitHub webhook payload, normalised
/// into a host-agnostic shape ready to be linked to one or more tasks.
/// </summary>
/// <param name="Repository">Full repository name, e.g. "owner/repo".</param>
/// <param name="RefType">Branch, commit or pull request.</param>
/// <param name="Title">Display title (commit message, PR title, or branch name).</param>
/// <param name="Url">Link to the reference on GitHub.</param>
/// <param name="Status">PR status, or None for branches/commits.</param>
/// <param name="ExternalId">Commit SHA or PR number.</param>
/// <param name="TextToScan">
/// The text to search for task references (typically the branch name plus the
/// commit message or PR title/body).
/// </param>
public sealed record ParsedDevelopmentRef(
    string Repository,
    DevelopmentRefType RefType,
    string Title,
    string Url,
    DevelopmentLinkStatus Status,
    string? ExternalId,
    string TextToScan);
