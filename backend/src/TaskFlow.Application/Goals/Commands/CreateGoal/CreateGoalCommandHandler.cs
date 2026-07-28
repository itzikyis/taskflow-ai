using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Goals.Commands.CreateGoal;

/// <summary>Handles <see cref="CreateGoalCommand"/>.</summary>
public sealed class CreateGoalCommandHandler(IGoalRepository goalRepository)
    : IRequestHandler<CreateGoalCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        CreateGoalCommand request,
        CancellationToken cancellationToken)
    {
        var result = Goal.Create(
            request.ProjectId,
            request.OwnerId,
            request.Title,
            request.Description,
            request.DueDate);

        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await goalRepository.AddAsync(result.Value!, cancellationToken);
        await goalRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
