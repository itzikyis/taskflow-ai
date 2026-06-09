using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.DeleteTask;

/// <summary>Command to permanently delete a task.</summary>
public sealed record DeleteTaskCommand(Guid TaskId) : IRequest<Result>;
