using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.UpdateBoard;

/// <summary>Updates a board's name.</summary>
public sealed record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result>;
