using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Commands.EditComment;

/// <summary>Edits an existing comment.</summary>
public sealed record EditCommentCommand(Guid CommentId, Guid RequesterId, string Content) : IRequest<Result>;
