using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GetStatusDigest;

/// <summary>Returns an AI-generated weekly project status digest for the given project.</summary>
/// <param name="ProjectId">The identifier of the project to summarise.</param>
/// <param name="PeriodDays">Number of past days to include in the digest window (1–90, default 7).</param>
public sealed record GetStatusDigestQuery(Guid ProjectId, int PeriodDays = 7)
    : IRequest<Result<StatusDigestDto>>;
