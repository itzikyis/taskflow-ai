using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.AssessSprintRisk;

/// <summary>Input task snapshot used for risk scoring.</summary>
public sealed record RiskTaskInput(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? UpdatedAt,
    int OpenBlockerCount);

/// <summary>Requests an AI-powered risk assessment for the supplied tasks.</summary>
public sealed record AssessSprintRiskQuery(IReadOnlyList<RiskTaskInput> Tasks)
    : IRequest<Result<SprintRiskAssessment>>;
