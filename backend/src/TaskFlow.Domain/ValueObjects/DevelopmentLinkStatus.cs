namespace TaskFlow.Domain.ValueObjects;

/// <summary>The lifecycle status of a linked development reference (mainly for pull requests).</summary>
public enum DevelopmentLinkStatus
{
    /// <summary>Not applicable (e.g. a plain branch or commit).</summary>
    None = 0,

    /// <summary>An open pull request.</summary>
    Open = 1,

    /// <summary>A pull request under review / marked as draft.</summary>
    Draft = 2,

    /// <summary>A merged pull request.</summary>
    Merged = 3,

    /// <summary>A closed (unmerged) pull request.</summary>
    Closed = 4
}
