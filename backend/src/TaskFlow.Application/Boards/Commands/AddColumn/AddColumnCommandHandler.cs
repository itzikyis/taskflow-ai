using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Boards.Commands.AddColumn;

/// <summary>Handles <see cref="AddColumnCommand"/>.</summary>
public sealed class AddColumnCommandHandler(IBoardRepository repo)
    : IRequestHandler<AddColumnCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddColumnCommand request, CancellationToken ct)
    {
        var board = await repo.GetByIdAsync(request.BoardId, ct);
        if (board is null) return Result<Guid>.Failure(BoardErrors.NotFound);
        var result = board.AddColumn(request.Name, request.Order, request.WipLimit);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
