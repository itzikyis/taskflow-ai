using MediatR;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestDueDate;

/// <summary>Asks the AI to suggest a due date for a task.</summary>
public sealed record SuggestDueDateQuery(string TaskTitle, string? TaskDescription) : IRequest<Result<AiSuggestionDto>>;
