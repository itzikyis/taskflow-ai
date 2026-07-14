using MediatR;
using TaskFlow.Application.Initiatives.Dtos;

namespace TaskFlow.Application.Initiatives.Queries.GetInitiativeRoadmap;

/// <summary>Returns all initiatives with rolled-up task progress.</summary>
public sealed record GetInitiativeRoadmapQuery : IRequest<IReadOnlyList<InitiativeDto>>;
