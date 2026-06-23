using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.UpdateTaskStatus;

/// <summary>Transitions a task to a new status.</summary>
public sealed record UpdateTaskStatusCommand(Guid TaskId, TaskItemStatus NewStatus)
    : IRequest<Result>;
