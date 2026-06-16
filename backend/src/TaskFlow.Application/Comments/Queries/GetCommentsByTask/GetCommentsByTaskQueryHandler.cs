using MediatR;
using TaskFlow.Application.Comments.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Comments.Queries.GetCommentsByTask;

/// <summary>Handles <see cref="GetCommentsByTaskQuery"/>.</summary>
public sealed class GetCommentsByTaskQueryHandler(ICommentRepository repo)
    : IRequestHandler<GetCommentsByTaskQuery, IReadOnlyList<CommentDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CommentDto>> Handle(GetCommentsByTaskQuery request, CancellationToken ct)
    {
        var comments = await repo.GetByTaskIdAsync(request.TaskId, ct);
        return comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.TaskId, c.AuthorId, c.Content, c.CreatedAt, c.UpdatedAt))
            .ToList();
    }
}
