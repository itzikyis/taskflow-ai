using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Handles <see cref="GetTeamWorkloadQuery"/>.</summary>
public sealed class GetTeamWorkloadQueryHandler(
    ITaskRepository taskRepository,
    ITeamRepository teamRepository,
    IUserRepository userRepository)
    : IRequestHandler<GetTeamWorkloadQuery, Result<TeamWorkloadDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TeamWorkloadDto>> Handle(GetTeamWorkloadQuery request, CancellationToken cancellationToken)
    {
        var allTasks = await taskRepository.GetAllAsync(cancellationToken: cancellationToken);
        var teams = await teamRepository.GetAllAsync(cancellationToken);

        // Collect all unique member user IDs across all teams
        var memberUserIds = teams
            .SelectMany(t => t.Members)
            .Select(m => m.UserId)
            .Distinct()
            .ToHashSet();

        var memberWorkloads = new List<MemberWorkloadDto>();

        foreach (var userId in memberUserIds)
        {
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            var displayName = user?.DisplayName ?? userId.ToString();

            var assignedTasks = allTasks.Where(t => t.AssignedToUserId == userId).ToList();

            var openTasks = assignedTasks.Count(t => t.Status == TaskItemStatus.Todo);
            var inProgressTasks = assignedTasks.Count(t => t.Status == TaskItemStatus.InProgress || t.Status == TaskItemStatus.InReview);
            var completedTasks = assignedTasks.Count(t => t.Status == TaskItemStatus.Done);

            memberWorkloads.Add(new MemberWorkloadDto(
                userId,
                displayName,
                openTasks,
                inProgressTasks,
                completedTasks,
                assignedTasks.Count));
        }

        var unassignedTasks = allTasks.Count(t => t.AssignedToUserId is null);

        return Result<TeamWorkloadDto>.Success(new TeamWorkloadDto(memberWorkloads, unassignedTasks));
    }
}
