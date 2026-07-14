using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>A markdown document scoped to a project, used as a wiki/specs space.</summary>
public sealed class ProjectDocument : AggregateRoot
{
    private ProjectDocument() { } // EF Core

    /// <summary>Project this document belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Document title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Markdown body content.</summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>User who created this document.</summary>
    public Guid AuthorId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>Creates a new project document.</summary>
    public static ProjectDocument Create(Guid projectId, string title, string body, Guid authorId)
    {
        var now = DateTime.UtcNow;
        return new ProjectDocument
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title.Trim(),
            Body = body,
            AuthorId = authorId,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>Updates title and body.</summary>
    public void Update(string title, string body)
    {
        Title = title.Trim();
        Body = body;
        UpdatedAt = DateTime.UtcNow;
    }
}
