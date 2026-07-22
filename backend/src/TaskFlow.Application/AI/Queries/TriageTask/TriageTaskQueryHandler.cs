using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>Handles <see cref="TriageTaskQuery"/> by loading task and team context, then delegating to the AI assistant.</summary>
public sealed class TriageTaskQueryHandler(
    IAiAssistantService ai,
    ITaskRepository taskRepository,
    ITeamRepository teamRepository,
    IUserRepository userRepository,
    ILogger<TriageTaskQueryHandler> logger)
    : IRequestHandler<TriageTaskQuery, Result<TriageTaskDto>>
{
    private static readonly TriageTaskDto FallbackDto =
        new(null, null, null, false, null, "AI triage is temporarily unavailable.");

    /// <inheritdoc/>
    public async Task<Result<TriageTaskDto>> Handle(TriageTaskQuery request, CancellationToken ct)
    {
        // Load the task being triaged.
        var task = await taskRepository.GetByIdAsync(request.TaskId, ct);
        if (task is null)
            return Result<TriageTaskDto>.Failure(new Error("Triage.TaskNotFound", $"Task {request.TaskId} was not found."));

        // Load recent tasks in the project for duplicate detection.
        // GetAllAsync does not filter by project; we take the last 20 created tasks as a reasonable context window.
        var allTasks = await taskRepository.GetAllAsync(cancellationToken: ct);
        var recentTasks = allTasks
            .Where(t => t.Id != request.TaskId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToList();

        // Load all teams to collect potential assignee candidates.
        var teams = await teamRepository.GetAllAsync(ct);
        var memberUserIds = teams
            .SelectMany(t => t.Members)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        // Resolve display names for candidate members.
        var memberNames = new List<(Guid Id, string Name)>();
        foreach (var userId in memberUserIds)
        {
            var user = await userRepository.GetByIdAsync(userId, ct);
            if (user is not null)
                memberNames.Add((user.Id, user.DisplayName));
        }

        try
        {
            var result = await ai.TriageTaskAsync(task.Title, task.Description, memberNames, recentTasks
                .Select(t => (t.Title, t.Description))
                .ToList(), ct);
            return Result<TriageTaskDto>.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI triage failed: service is misconfigured.");
            return Result<TriageTaskDto>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI triage failed for task {TaskId}.", request.TaskId);
            // Graceful degradation — return a neutral DTO rather than propagating the error.
            return Result<TriageTaskDto>.Success(FallbackDto);
        }
    }
}
