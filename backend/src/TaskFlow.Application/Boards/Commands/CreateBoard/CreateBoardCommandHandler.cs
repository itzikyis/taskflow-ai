using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.CreateBoard;

/// <summary>Handles <see cref="CreateBoardCommand"/>.</summary>
public sealed class CreateBoardCommandHandler(IBoardRepository repo)
    : IRequestHandler<CreateBoardCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBoardCommand request, CancellationToken ct)
    {
        var result = Board.Create(request.Name, request.ProjectId);
        if (result.IsFailure) return Result<Guid>.Failure(result.Error);
        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }
}
