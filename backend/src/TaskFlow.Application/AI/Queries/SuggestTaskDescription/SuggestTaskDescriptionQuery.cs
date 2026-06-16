using MediatR;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestTaskDescription;

/// <summary>Asks the AI to suggest a description for a task given its title.</summary>
public sealed record SuggestTaskDescriptionQuery(string TaskTitle) : IRequest<Result<AiSuggestionDto>>;
