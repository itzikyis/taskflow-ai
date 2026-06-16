using MediatR;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SummarizeComments;

/// <summary>Asks the AI to summarize the comments on a task.</summary>
public sealed record SummarizeCommentsQuery(IReadOnlyList<string> Comments) : IRequest<Result<AiSuggestionDto>>;
