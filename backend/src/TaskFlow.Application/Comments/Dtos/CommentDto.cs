namespace TaskFlow.Application.Comments.Dtos;

/// <summary>DTO representing a comment.</summary>
public sealed record CommentDto(Guid Id, Guid TaskId, Guid AuthorId, string Content, DateTime CreatedAt, DateTime? UpdatedAt);
