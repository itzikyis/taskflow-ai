namespace TaskFlow.Domain.ValueObjects;

/// <summary>The kind of source-control reference linked to a task.</summary>
public enum DevelopmentRefType
{
    /// <summary>A git branch.</summary>
    Branch = 0,

    /// <summary>A single commit.</summary>
    Commit = 1,

    /// <summary>A pull request.</summary>
    PullRequest = 2
}
