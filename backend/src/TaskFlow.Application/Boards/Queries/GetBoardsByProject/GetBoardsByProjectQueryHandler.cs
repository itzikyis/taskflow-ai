using MediatR;
using TaskFlow.Application.Boards.Dtos;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Boards.Queries.GetBoardsByProject;

/// <summary>Handles <see cref="GetBoardsByProjectQuery"/>.</summary>
public sealed class GetBoardsByProjectQueryHandler(IBoardRepository repo)
    : IRequestHandler<GetBoardsByProjectQuery, IReadOnlyList<BoardDto>>
{
    public async Task<IReadOnlyList<BoardDto>> Handle(GetBoardsByProjectQuery request, CancellationToken ct)
    {
        var boards = await repo.GetByProjectIdAsync(request.ProjectId, ct);
        return boards.Select(b => new BoardDto(
            b.Id, b.Name, b.ProjectId,
            b.Columns.OrderBy(c => c.Order)
                     .Select(c => new BoardColumnDto(c.Id, c.Name, c.Order, c.WipLimit))
                     .ToList(),
            b.CreatedAt, b.UpdatedAt)).ToList();
    }
}
