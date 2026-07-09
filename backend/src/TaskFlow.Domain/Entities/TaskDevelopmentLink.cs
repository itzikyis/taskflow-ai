using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root linking a task to a source-control reference (branch, commit
/// or pull request) so engineering activity is visible on the task.
/// </summary>
public sealed class TaskDevelopmentLink : AggregateRoot
{
    private TaskDevelopmentLink() { } // EF Core constructor

    private TaskDevelopmentLink(
        Guid id,
        Guid taskId,
        string repository,
        DevelopmentRefType refType,
        string title,
        string url,
        DevelopmentLinkStatus status,
        string? externalId)
    {
        Id = id;
        TaskId = taskId;
        Repository = repository;
        RefType = refType;
        Title = title;
        Url = url;
        Status = status;
        ExternalId = externalId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the task this link belongs to.</summary>
    public Guid TaskId { get; private init; }

    /// <summary>Gets the repository this reference lives in (e.g. "owner/repo").</summary>
    public string Repository { get; private init; } = string.Empty;

    /// <summary>Gets the kind of reference (branch, commit, pull request).</summary>
    public DevelopmentRefType RefType { get; private init; }

    /// <summary>Gets a human-readable title (branch name, commit message, or PR title).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the URL to the reference on the code host.</summary>
    public string Url { get; private init; } = string.Empty;

    /// <summary>Gets the status of the reference (mainly meaningful for pull requests).</summary>
    public DevelopmentLinkStatus Status { get; private set; }

    /// <summary>Gets the host-specific identifier (commit SHA or PR number), if any.</summary>
    public string? ExternalId { get; private init; }

    /// <summary>Gets the UTC timestamp when the link was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the UTC timestamp of the last status update, if any.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Creates a new development link after validating its inputs.</summary>
    public static Result<TaskDevelopmentLink> Create(
        Guid taskId,
        string repository,
        DevelopmentRefType refType,
        string title,
        string url,
        DevelopmentLinkStatus status = DevelopmentLinkStatus.None,
        string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(repository))
            return Result<TaskDevelopmentLink>.Failure(DevelopmentLinkErrors.RepositoryRequired);

        if (string.IsNullOrWhiteSpace(title))
            return Result<TaskDevelopmentLink>.Failure(DevelopmentLinkErrors.TitleRequired);

        if (string.IsNullOrWhiteSpace(url))
            return Result<TaskDevelopmentLink>.Failure(DevelopmentLinkErrors.UrlRequired);

        return Result<TaskDevelopmentLink>.Success(new TaskDevelopmentLink(
            Guid.NewGuid(), taskId, repository.Trim(), refType,
            title.Trim(), url.Trim(), status, externalId?.Trim()));
    }

    /// <summary>Updates the reference status and title (e.g. when a PR is merged).</summary>
    public void Update(DevelopmentLinkStatus status, string? title = null)
    {
        Status = status;
        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
