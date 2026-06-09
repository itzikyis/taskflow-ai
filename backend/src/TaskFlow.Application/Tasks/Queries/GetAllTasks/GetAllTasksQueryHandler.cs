using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Tasks.Queries.GetTaskById;

namespace TaskFlow.Application.Tasks.Queries.GetAllTasks;

/// <summary>Handles <see cref="GetAllTasksQuery"/>.</summary>
internal sealed class GetAllTasksQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetAllTasksQuery, IReadOnlyList<TaskDto>>
{
    public async Task<IReadOnlyList<TaskDto>> Handle(
        GetAllTasksQuery request,
        CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.GetAllAsync(request.AssignedToUserId, cancellationToken);

        return tasks.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.AssignedToUserId,
            t.CreatedByUserId,
            t.CreatedAt,
            t.UpdatedAt)).ToList();
    }
}
