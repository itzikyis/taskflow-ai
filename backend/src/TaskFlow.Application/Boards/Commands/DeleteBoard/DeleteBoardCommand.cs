using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.DeleteBoard;

/// <summary>Deletes a board permanently.</summary>
public sealed record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;
