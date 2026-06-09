using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.UpdateTask;

/// <summary>Command to update a task's title and description.</summary>
public sealed record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description) : IRequest<Result>;
