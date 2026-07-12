using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Dependencies.Commands.AddDependency;

/// <summary>Command to declare that <see cref="TaskId"/> is blocked by <see cref="BlockedByTaskId"/>.</summary>
public sealed record AddDependencyCommand(Guid TaskId, Guid BlockedByTaskId) : IRequest<Result<Guid>>;
