using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Commands.LogTime;

/// <summary>Command to log time against a task.</summary>
public sealed record LogTimeCommand(
    Guid TaskId,
    Guid UserId,
    int Minutes,
    string? Note,
    DateTime? LoggedAt = null) : IRequest<Result<Guid>>;
