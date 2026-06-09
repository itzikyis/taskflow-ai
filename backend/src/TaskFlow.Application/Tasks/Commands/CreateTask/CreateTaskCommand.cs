using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.CreateTask;

/// <summary>Command to create a new task.</summary>
public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    TaskPriority Priority,
    Guid CreatedByUserId) : IRequest<Result<Guid>>;
