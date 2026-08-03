using MediatR;
using TaskFlow.Application.TimeTracking.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Queries.GetTimeReport;

/// <summary>Returns an estimate-vs-actual time report for all tasks in a project.</summary>
/// <param name="ProjectId">The project to report on.</param>
/// <param name="From">Optional inclusive UTC start of the logged-time window.</param>
/// <param name="To">Optional inclusive UTC end of the logged-time window.</param>
public sealed record GetTimeReportQuery(
    Guid ProjectId,
    DateTime? From,
    DateTime? To) : IRequest<Result<TimeReportDto>>;
