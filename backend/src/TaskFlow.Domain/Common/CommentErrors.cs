namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for comment management.</summary>
public static class CommentErrors
{
    /// <summary>Returned when comment content is empty or whitespace.</summary>
    public static readonly Error ContentRequired = new("Comment.ContentRequired", "Comment content is required.");

    /// <summary>Returned when comment content exceeds 5000 characters.</summary>
    public static readonly Error ContentTooLong = new("Comment.ContentTooLong", "Comment content must not exceed 5000 characters.");

    /// <summary>Returned when the requested comment does not exist.</summary>
    public static readonly Error NotFound = new("Comment.NotFound", "Comment was not found.");

    /// <summary>Returned when the requester is not the comment author.</summary>
    public static readonly Error NotOwner = new("Comment.NotOwner", "Only the comment author can modify this comment.");
}
