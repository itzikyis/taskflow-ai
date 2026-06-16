using MediatR;
using TaskFlow.Application.Boards.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Queries.GetBoardById;

/// <summary>Handles <see cref="GetBoardByIdQuery"/>.</summary>
public sealed class GetBoardByIdQueryHandler(IBoardRepository repo)
    : IRequestHandler<GetBoardByIdQuery, Result<BoardDto>>
{
    public async Task<Result<BoardDto>> Handle(GetBoardByIdQuery request, CancellationToken ct)
    {
        var board = await repo.GetByIdAsync(request.BoardId, ct);
        if (board is null) return Result<BoardDto>.Failure(BoardErrors.NotFound);
        return Result<BoardDto>.Success(ToDto(board));
    }

    private static BoardDto ToDto(Domain.Entities.Board b) => new(
        b.Id, b.Name, b.ProjectId,
        b.Columns.OrderBy(c => c.Order)
                 .Select(c => new BoardColumnDto(c.Id, c.Name, c.Order, c.WipLimit))
                 .ToList(),
        b.CreatedAt, b.UpdatedAt);
}
