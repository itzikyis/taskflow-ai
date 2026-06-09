using MediatR;
using TaskFlow.Application.Tasks.Queries.GetTaskById;

namespace TaskFlow.Application.Tasks.Queries.GetAllTasks;

/// <summary>Query to list all tasks, optionally filtered by assigned user.</summary>
public sealed record GetAllTasksQuery(Guid? AssignedToUserId = null)
    : IRequest<IReadOnlyList<TaskDto>>;
