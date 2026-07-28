using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Goals.Commands.UpdateGoalProgress;

/// <summary>Command that updates the progress percentage and status of a Goal.</summary>
public sealed record UpdateGoalProgressCommand(
    Guid GoalId,
    int ProgressPercent,
    string Status) : IRequest<Result>;
