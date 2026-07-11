using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.CreateSubtasks;

/// <summary>Handles <see cref="CreateSubtasksCommand"/> by creating child tasks under a parent.</summary>
public sealed class CreateSubtasksCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<CreateSubtasksCommand, Result<IReadOnlyList<Guid>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<Guid>>> Handle(CreateSubtasksCommand request, CancellationToken ct)
    {
        var parent = await taskRepository.GetByIdAsync(request.ParentTaskId, ct);
        if (parent is null)
            return Result<IReadOnlyList<Guid>>.Failure(TaskErrors.NotFound);

        var createdIds = new List<Guid>();

        foreach (var input in request.Subtasks)
        {
            var result = TaskItem.Create(input.Title, input.Description, TaskPriority.Medium, request.CreatedByUserId);
            if (result.IsFailure)
                return Result<IReadOnlyList<Guid>>.Failure(result.Error);

            var subtask = result.Value!;
            subtask.SetParent(request.ParentTaskId);
            await taskRepository.AddAsync(subtask, ct);
            createdIds.Add(subtask.Id);
        }

        await taskRepository.SaveChangesAsync(ct);
        return Result<IReadOnlyList<Guid>>.Success(createdIds);
    }
}
