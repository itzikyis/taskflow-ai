using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Dependencies.Commands.RemoveDependency;

/// <summary>Command to remove a task dependency.</summary>
public sealed record RemoveDependencyCommand(Guid DependencyId) : IRequest<Result>;
