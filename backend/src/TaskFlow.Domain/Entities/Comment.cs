using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

/// <summary>A comment left on a task by a user.</summary>
public sealed class Comment : AggregateRoot
{
    private Comment() { }

    private Comment(Guid id, Guid taskId, Guid authorId, string content)
    {
        Id = id;
        TaskId = taskId;
        AuthorId = authorId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new CommentAddedEvent(id, taskId, authorId));
    }

    /// <summary>Gets the task this comment belongs to.</summary>
    public Guid TaskId { get; private init; }

    /// <summary>Gets the author's user id.</summary>
    public Guid AuthorId { get; private init; }

    /// <summary>Gets the comment text.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>Gets the creation timestamp.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the last edit timestamp.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Creates a new comment.</summary>
    public static Result<Comment> Create(Guid taskId, Guid authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Result<Comment>.Failure(CommentErrors.ContentRequired);
        if (content.Length > 5000) return Result<Comment>.Failure(CommentErrors.ContentTooLong);
        return Result<Comment>.Success(new Comment(Guid.NewGuid(), taskId, authorId, content.Trim()));
    }

    /// <summary>Edits the comment content. Only the original author may edit.</summary>
    public Result Edit(Guid requesterId, string content)
    {
        if (requesterId != AuthorId) return Result.Failure(CommentErrors.NotOwner);
        if (string.IsNullOrWhiteSpace(content)) return Result.Failure(CommentErrors.ContentRequired);
        if (content.Length > 5000) return Result.Failure(CommentErrors.ContentTooLong);
        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }
}
