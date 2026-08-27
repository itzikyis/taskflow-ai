using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Handles <see cref="GetTeamWorkloadQuery"/>.</summary>
public sealed class GetTeamWorkloadQueryHandler(
    ITaskRepository taskRepository,
    ITeamRepository teamRepository,
    IUserRepository userRepository,
    ITimeEntryRepository timeEntryRepository)
    : IRequestHandler<GetTeamWorkloadQuery, Result<TeamWorkloadDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TeamWorkloadDto>> Handle(GetTeamWorkloadQuery request, CancellationToken cancellationToken)
    {
        var allTasks = await taskRepository.GetAllAsync(cancellationToken: cancellationToken);
        var teams = await teamRepository.GetAllAsync(cancellationToken);

        // ISO week boundaries (Monday 00:00 UTC → Sunday 23:59:59.999 UTC)
        var now = DateTime.UtcNow;
        var dayOfWeek = (int)now.DayOfWeek;
        var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var weekStart = now.Date.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(7).AddTicks(-1);

        // Collect all unique member user IDs across all teams
        var memberUserIds = teams
            .SelectMany(t => t.Members)
            .Select(m => m.UserId)
            .Distinct()
            .ToHashSet();

        var capacityHours = request.CapacityHoursPerWeek > 0 ? request.CapacityHoursPerWeek : 40;

        var memberWorkloads = new List<MemberWorkloadDto>();

        foreach (var userId in memberUserIds)
        {
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            var displayName = user?.DisplayName ?? userId.ToString();

            var assignedTasks = allTasks.Where(t => t.AssignedToUserId == userId).ToList();

            var openTasks = assignedTasks.Count(t => t.Status == TaskItemStatus.Todo);
            var inProgressTasks = assignedTasks.Count(t =>
                t.Status == TaskItemStatus.InProgress || t.Status == TaskItemStatus.InReview);
            var completedTasks = assignedTasks.Count(t => t.Status == TaskItemStatus.Done);

            // Fetch this week's time entries for the member
            var weekEntries = await timeEntryRepository.GetByUserAndDateRangeAsync(
                userId, weekStart, weekEnd, cancellationToken);

            var loggedMinutes = weekEntries.Sum(e => e.Minutes);
            var loggedHours = loggedMinutes / 60.0;
            var utilizationPercent = capacityHours > 0
                ? Math.Round(loggedHours / capacityHours * 100, 1)
                : 0;

            memberWorkloads.Add(new MemberWorkloadDto(
                userId,
                displayName,
                openTasks,
                inProgressTasks,
                completedTasks,
                assignedTasks.Count,
                capacityHours,
                Math.Round(loggedHours, 2),
                utilizationPercent,
                loggedHours > capacityHours));
        }

        var unassignedTasks = allTasks.Count(t => t.AssignedToUserId is null);
        var totalCapacity = memberWorkloads.Count * capacityHours;
        var totalLogged = memberWorkloads.Sum(m => m.LoggedHoursThisWeek);

        return Result<TeamWorkloadDto>.Success(
            new TeamWorkloadDto(memberWorkloads, unassignedTasks, totalCapacity, Math.Round(totalLogged, 2)));
    }
}
