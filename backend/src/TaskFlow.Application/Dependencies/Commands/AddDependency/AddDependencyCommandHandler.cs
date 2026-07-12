using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Dependencies.Commands.AddDependency;

/// <summary>Handles <see cref="AddDependencyCommand"/>, guarding against self, duplicate and cyclic edges.</summary>
public sealed class AddDependencyCommandHandler(
    ITaskDependencyRepository repo,
    ITaskRepository taskRepository)
    : IRequestHandler<AddDependencyCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(AddDependencyCommand request, CancellationToken ct)
    {
        if (request.TaskId == request.BlockedByTaskId)
            return Result<Guid>.Failure(TaskDependencyErrors.SelfDependency);

        if (await taskRepository.GetByIdAsync(request.TaskId, ct) is null ||
            await taskRepository.GetByIdAsync(request.BlockedByTaskId, ct) is null)
            return Result<Guid>.Failure(TaskErrors.NotFound);

        if (await repo.ExistsAsync(request.TaskId, request.BlockedByTaskId, ct))
            return Result<Guid>.Failure(TaskDependencyErrors.Duplicate);

        var all = await repo.GetAllAsync(ct);
        if (WouldCreateCycle(all, request.TaskId, request.BlockedByTaskId))
            return Result<Guid>.Failure(TaskDependencyErrors.Cycle);

        var result = TaskDependency.Create(request.TaskId, request.BlockedByTaskId);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        await repo.AddAsync(result.Value!, ct);
        await repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(result.Value!.Id);
    }

    /// <summary>
    /// Adding "task is blocked by prerequisite" creates the edge prerequisite → task.
    /// It forms a cycle if <paramref name="prerequisite"/> is already reachable from
    /// <paramref name="task"/> along existing prerequisite → dependent edges.
    /// </summary>
    private static bool WouldCreateCycle(
        IReadOnlyList<TaskDependency> all, Guid task, Guid prerequisite)
    {
        // adjacency: blocker -> list of blocked tasks
        var adjacency = all
            .GroupBy(d => d.BlockedByTaskId)
            .ToDictionary(g => g.Key, g => g.Select(d => d.TaskId).ToList());

        var stack = new Stack<Guid>();
        var visited = new HashSet<Guid>();
        stack.Push(task);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == prerequisite)
                return true;
            if (!visited.Add(current))
                continue;
            if (adjacency.TryGetValue(current, out var next))
                foreach (var n in next)
                    stack.Push(n);
        }

        return false;
    }
}
