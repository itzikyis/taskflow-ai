using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.MoveTaskToColumn;

/// <summary>Places a task into a board column (or removes it from any column when columnId is null).</summary>
public sealed record MoveTaskToColumnCommand(Guid TaskId, Guid? ColumnId, Guid ActorId) : IRequest<Result>;
