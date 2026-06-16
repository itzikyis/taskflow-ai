using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Commands.AddComment;

/// <summary>Adds a comment to a task.</summary>
public sealed record AddCommentCommand(Guid TaskId, Guid AuthorId, string Content) : IRequest<Result<Guid>>;
