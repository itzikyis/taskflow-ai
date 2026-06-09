using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Queries.GetTaskById;

/// <summary>Query to retrieve a single task by ID.</summary>
public sealed record GetTaskByIdQuery(Guid TaskId) : IRequest<Result<TaskDto>>;
