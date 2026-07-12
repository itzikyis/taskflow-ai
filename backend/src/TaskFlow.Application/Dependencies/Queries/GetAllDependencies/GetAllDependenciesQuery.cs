using MediatR;
using TaskFlow.Application.Dependencies.Dtos;

namespace TaskFlow.Application.Dependencies.Queries.GetAllDependencies;

/// <summary>Query returning every dependency edge (for the timeline view).</summary>
public sealed record GetAllDependenciesQuery : IRequest<IReadOnlyList<DependencyEdgeDto>>;
