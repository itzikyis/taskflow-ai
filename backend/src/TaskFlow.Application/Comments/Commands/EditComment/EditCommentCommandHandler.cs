using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Commands.EditComment;

/// <summary>Handles <see cref="EditCommentCommand"/>.</summary>
public sealed class EditCommentCommandHandler(ICommentRepository repo)
    : IRequestHandler<EditCommentCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(EditCommentCommand request, CancellationToken ct)
    {
        var comment = await repo.GetByIdAsync(request.CommentId, ct);
        if (comment is null) return Result.Failure(CommentErrors.NotFound);
        var result = comment.Edit(request.RequesterId, request.Content);
        if (result.IsFailure) return result;
        repo.Update(comment);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
