using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GenerateRetrospective;

/// <summary>A task summary supplied as input to the retrospective generator.</summary>
public sealed record RetroTaskSummary(string Title, string? Description, string Priority);

/// <summary>Query that generates a sprint retrospective draft from sprint work.</summary>
public sealed record GenerateSprintRetrospectiveQuery(
    IReadOnlyList<RetroTaskSummary> Completed,
    IReadOnlyList<RetroTaskSummary> Incomplete) : IRequest<Result<SprintRetrospective>>;
