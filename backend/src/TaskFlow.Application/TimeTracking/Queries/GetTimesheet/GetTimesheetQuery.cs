using MediatR;
using TaskFlow.Application.TimeTracking.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Queries.GetTimesheet;

/// <summary>Query returning a weekly timesheet for a given user.</summary>
/// <param name="UserId">The user whose time entries are aggregated.</param>
/// <param name="WeekStart">Monday of the target week (inclusive).</param>
public sealed record GetTimesheetQuery(Guid UserId, DateOnly WeekStart)
    : IRequest<Result<TimesheetDto>>;
