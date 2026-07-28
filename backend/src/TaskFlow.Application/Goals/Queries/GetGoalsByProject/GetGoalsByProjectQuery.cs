using MediatR;

namespace TaskFlow.Application.Goals.Queries.GetGoalsByProject;

/// <summary>Query that returns all goals for the specified project.</summary>
public sealed record GetGoalsByProjectQuery(Guid ProjectId) : IRequest<IReadOnlyList<GoalDto>>;
