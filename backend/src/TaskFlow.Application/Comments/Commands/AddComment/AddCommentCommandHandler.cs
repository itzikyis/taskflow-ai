using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Comments.Commands.AddComment;

/// <summary>Handles <see cref="AddCommentCommand"/>.</summary>
public sealed class AddCommentCommandHandler(ICommentRepository repo)
    : IRequestHandler<AddCommentCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var result = Comment.Create(request.TaskId, request.AuthorId, request.Content);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);
        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
