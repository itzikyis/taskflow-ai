using MediatR;
using TaskFlow.Application.Dependencies.Dtos;

namespace TaskFlow.Application.Dependencies.Queries.GetTaskDependencies;

/// <summary>Query returning the blockers and blocked tasks for a task.</summary>
public sealed record GetTaskDependenciesQuery(Guid TaskId) : IRequest<TaskDependenciesDto>;
