using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.RemoveColumn;

/// <summary>Handles <see cref="RemoveColumnCommand"/>.</summary>
public sealed class RemoveColumnCommandHandler(IBoardRepository repo)
    : IRequestHandler<RemoveColumnCommand, Result>
{
    public async Task<Result> Handle(RemoveColumnCommand request, CancellationToken ct)
    {
        var board = await repo.GetByIdAsync(request.BoardId, ct);
        if (board is null) return Result.Failure(BoardErrors.NotFound);
        var result = board.RemoveColumn(request.ColumnId);
        if (result.IsFailure) return result;
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
