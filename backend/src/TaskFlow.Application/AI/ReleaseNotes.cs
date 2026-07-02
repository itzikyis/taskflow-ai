namespace TaskFlow.Application.AI;

/// <summary>Generated release notes for a specific version.</summary>
/// <param name="Version">The version identifier (e.g. "1.0.0").</param>
/// <param name="Summary">A 1–2 sentence overview of the release.</param>
/// <param name="Features">List of new feature descriptions.</param>
/// <param name="BugFixes">List of bug fix descriptions.</param>
/// <param name="Improvements">List of improvement descriptions.</param>
/// <param name="MarkdownContent">Full Markdown-formatted release notes.</param>
public sealed record ReleaseNotes(
    string Version,
    string Summary,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> BugFixes,
    IReadOnlyList<string> Improvements,
    string MarkdownContent);
