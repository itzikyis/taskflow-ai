using MediatR;
using TaskFlow.Application.Dependencies.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Dependencies.Queries.GetTaskDependencies;

/// <summary>Handles <see cref="GetTaskDependenciesQuery"/>.</summary>
public sealed class GetTaskDependenciesQueryHandler(
    ITaskDependencyRepository repo,
    ITaskRepository taskRepository)
    : IRequestHandler<GetTaskDependenciesQuery, TaskDependenciesDto>
{
    /// <inheritdoc/>
    public async Task<TaskDependenciesDto> Handle(GetTaskDependenciesQuery request, CancellationToken ct)
    {
        var deps = await repo.GetByTaskAsync(request.TaskId, ct);
        var tasks = await taskRepository.GetAllAsync(null, ct);
        var byId = tasks.ToDictionary(t => t.Id);

        DependencyTaskDto? Map(Guid dependencyId, Guid taskId) =>
            byId.TryGetValue(taskId, out var t)
                ? new DependencyTaskDto(dependencyId, t.Id, t.Title, t.Status.ToString())
                : null;

        var blockedBy = deps
            .Where(d => d.TaskId == request.TaskId)
            .Select(d => Map(d.Id, d.BlockedByTaskId))
            .OfType<DependencyTaskDto>()
            .ToList();

        var blocks = deps
            .Where(d => d.BlockedByTaskId == request.TaskId)
            .Select(d => Map(d.Id, d.TaskId))
            .OfType<DependencyTaskDto>()
            .ToList();

        return new TaskDependenciesDto(request.TaskId, blockedBy, blocks);
    }
}
