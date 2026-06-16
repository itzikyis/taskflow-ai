using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Commands.DeleteComment;

/// <summary>Deletes a comment.</summary>
public sealed record DeleteCommentCommand(Guid CommentId, Guid RequesterId) : IRequest<Result>;
