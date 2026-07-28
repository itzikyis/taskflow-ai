using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Goals.Commands.UpdateGoalProgress;

/// <summary>Handles <see cref="UpdateGoalProgressCommand"/>.</summary>
public sealed class UpdateGoalProgressCommandHandler(IGoalRepository goalRepository)
    : IRequestHandler<UpdateGoalProgressCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        UpdateGoalProgressCommand request,
        CancellationToken cancellationToken)
    {
        var goal = await goalRepository.GetByIdAsync(request.GoalId, cancellationToken);
        if (goal is null)
            return Result.Failure(GoalErrors.NotFound);

        var updateResult = goal.UpdateProgress(request.ProgressPercent);
        if (updateResult.IsFailure)
            return updateResult;

        goal.SetStatus(request.Status);
        goalRepository.Update(goal);
        await goalRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
