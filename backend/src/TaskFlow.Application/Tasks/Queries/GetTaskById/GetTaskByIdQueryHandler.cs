using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Queries.GetTaskById;

/// <summary>Handles <see cref="GetTaskByIdQuery"/>.</summary>
internal sealed class GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    public async Task<Result<TaskDto>> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result<TaskDto>.Failure(TaskErrors.NotFound);

        var dto = new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.AssignedToUserId,
            task.CreatedByUserId,
            task.CreatedAt,
            task.UpdatedAt,
            task.ColumnId,
            task.ParentTaskId);

        return Result<TaskDto>.Success(dto);
    }
}
