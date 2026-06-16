using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.DeleteBoard;

/// <summary>Handles <see cref="DeleteBoardCommand"/>.</summary>
public sealed class DeleteBoardCommandHandler(IBoardRepository repo)
    : IRequestHandler<DeleteBoardCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardCommand request, CancellationToken ct)
    {
        var board = await repo.GetByIdAsync(request.BoardId, ct);
        if (board is null) return Result.Failure(BoardErrors.NotFound);
        repo.Remove(board);
        await repo.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
