using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Goals.Commands.DeleteGoal;

/// <summary>Handles <see cref="DeleteGoalCommand"/>.</summary>
public sealed class DeleteGoalCommandHandler(IGoalRepository goalRepository)
    : IRequestHandler<DeleteGoalCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        DeleteGoalCommand request,
        CancellationToken cancellationToken)
    {
        var goal = await goalRepository.GetByIdAsync(request.GoalId, cancellationToken);
        if (goal is null)
            return Result.Failure(GoalErrors.NotFound);

        await goalRepository.DeleteAsync(request.GoalId, cancellationToken);
        await goalRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
