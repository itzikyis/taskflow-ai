using MediatR;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestTaskDescription;

/// <summary>Handles <see cref="SuggestTaskDescriptionQuery"/>.</summary>
public sealed class SuggestTaskDescriptionQueryHandler(IAiAssistantService ai)
    : IRequestHandler<SuggestTaskDescriptionQuery, Result<AiSuggestionDto>>
{
    public async Task<Result<AiSuggestionDto>> Handle(SuggestTaskDescriptionQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TaskTitle))
            return Result<AiSuggestionDto>.Failure(new Error("AI.TitleRequired", "Task title is required."));
        var suggestion = await ai.SuggestTaskDescriptionAsync(request.TaskTitle, ct);
        return Result<AiSuggestionDto>.Success(new AiSuggestionDto(suggestion));
    }
}
