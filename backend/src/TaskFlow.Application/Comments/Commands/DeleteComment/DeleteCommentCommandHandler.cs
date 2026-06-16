using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Commands.DeleteComment;

/// <summary>Handles <see cref="DeleteCommentCommand"/>.</summary>
public sealed class DeleteCommentCommandHandler(ICommentRepository repo)
    : IRequestHandler<DeleteCommentCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken ct)
    {
        var comment = await repo.GetByIdAsync(request.CommentId, ct);
        if (comment is null) return Result.Failure(CommentErrors.NotFound);
        if (comment.AuthorId != request.RequesterId) return Result.Failure(CommentErrors.NotOwner);
        repo.Remove(comment);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
