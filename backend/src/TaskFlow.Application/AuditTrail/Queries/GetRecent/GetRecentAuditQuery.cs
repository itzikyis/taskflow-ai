using MediatR;
using TaskFlow.Application.AuditTrail.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Queries.GetRecent;

/// <summary>
/// Query that returns the most recent audit entries across all entities, paged and ordered newest-first.
/// </summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of entries per page.</param>
public sealed record GetRecentAuditQuery(
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<AuditEntryDto>>>;
