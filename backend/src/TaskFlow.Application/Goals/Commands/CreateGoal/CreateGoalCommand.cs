using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Goals.Commands.CreateGoal;

/// <summary>Command that creates a new Goal (Objective).</summary>
public sealed record CreateGoalCommand(
    Guid ProjectId,
    Guid OwnerId,
    string Title,
    string? Description,
    DateTime? DueDate) : IRequest<Result<Guid>>;
