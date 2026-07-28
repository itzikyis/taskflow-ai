using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Goals.Commands.DeleteGoal;

/// <summary>Command that deletes a Goal by its identifier.</summary>
public sealed record DeleteGoalCommand(Guid GoalId) : IRequest<Result>;
