using MediatR;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Goals.Queries.GetGoalsByProject;

/// <summary>Handles <see cref="GetGoalsByProjectQuery"/>.</summary>
public sealed class GetGoalsByProjectQueryHandler(IGoalRepository goalRepository)
    : IRequestHandler<GetGoalsByProjectQuery, IReadOnlyList<GoalDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<GoalDto>> Handle(
        GetGoalsByProjectQuery request,
        CancellationToken cancellationToken)
    {
        var goals = await goalRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        return goals.Select(g => new GoalDto(
            g.Id,
            g.ProjectId,
            g.Title,
            g.Description,
            g.Status,
            g.ProgressPercent,
            g.DueDate,
            g.KeyResults.Select(kr => new KeyResultDto(
                kr.Id,
                kr.Title,
                kr.TargetValue,
                kr.CurrentValue,
                kr.Unit,
                kr.ProgressPercent)).ToList()
        )).ToList();
    }
}
