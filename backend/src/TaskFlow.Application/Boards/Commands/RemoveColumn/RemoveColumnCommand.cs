using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.RemoveColumn;

/// <summary>Removes a column from a board.</summary>
public sealed record RemoveColumnCommand(Guid BoardId, Guid ColumnId) : IRequest<Result>;
