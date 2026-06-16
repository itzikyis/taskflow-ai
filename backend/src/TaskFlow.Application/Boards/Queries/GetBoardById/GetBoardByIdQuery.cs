using MediatR;
using TaskFlow.Application.Boards.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Queries.GetBoardById;

/// <summary>Returns a board by id with all of its columns.</summary>
public sealed record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<BoardDto>>;
