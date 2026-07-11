using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestTaskBreakdown;

/// <summary>Query that asks the AI to break a task down into suggested subtasks.</summary>
public sealed record SuggestTaskBreakdownQuery(string Title, string? Description)
    : IRequest<Result<IReadOnlyList<SubtaskSuggestion>>>;
