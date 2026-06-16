using MediatR;
using TaskFlow.Application.Comments.Dtos;

namespace TaskFlow.Application.Comments.Queries.GetCommentsByTask;

/// <summary>Returns all comments for a task, ordered by creation time ascending.</summary>
public sealed record GetCommentsByTaskQuery(Guid TaskId) : IRequest<IReadOnlyList<CommentDto>>;
