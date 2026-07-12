using MediatR;
using TaskFlow.Application.TimeTracking.Dtos;

namespace TaskFlow.Application.TimeTracking.Queries.GetTaskTimeSummary;

/// <summary>Query returning all time entries for a task plus the total.</summary>
public sealed record GetTaskTimeSummaryQuery(Guid TaskId) : IRequest<TaskTimeSummaryDto>;
