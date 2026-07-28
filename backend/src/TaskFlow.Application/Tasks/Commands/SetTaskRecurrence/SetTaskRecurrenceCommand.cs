using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Commands.SetTaskRecurrence;

/// <summary>Sets the recurrence pattern on an existing task.</summary>
/// <param name="TaskId">The task to update.</param>
/// <param name="Pattern">One of "daily", "weekly", or "monthly".</param>
/// <param name="EndDate">Optional UTC date at which recurrence stops.</param>
public sealed record SetTaskRecurrenceCommand(Guid TaskId, string Pattern, DateTime? EndDate)
    : IRequest<Result>;
