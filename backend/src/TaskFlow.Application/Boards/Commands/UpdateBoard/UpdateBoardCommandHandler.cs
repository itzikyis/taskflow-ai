using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.UpdateBoard;

/// <summary>Handles <see cref="UpdateBoardCommand"/>.</summary>
public sealed class UpdateBoardCommandHandler(IBoardRepository repo)
    : IRequestHandler<UpdateBoardCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardCommand request, CancellationToken ct)
    {
        var board = await repo.GetByIdAsync(request.BoardId, ct);
        if (board is null) return Result.Failure(BoardErrors.NotFound);
        var result = board.Update(request.Name);
        if (result.IsFailure) return result;
        repo.Update(board);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
