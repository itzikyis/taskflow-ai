using MediatR;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestDueDate;

/// <summary>Handles <see cref="SuggestDueDateQuery"/>.</summary>
public sealed class SuggestDueDateQueryHandler(IAiAssistantService ai)
    : IRequestHandler<SuggestDueDateQuery, Result<AiSuggestionDto>>
{
    public async Task<Result<AiSuggestionDto>> Handle(SuggestDueDateQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TaskTitle))
            return Result<AiSuggestionDto>.Failure(new Error("AI.TitleRequired", "Task title is required."));
        var suggestion = await ai.SuggestDueDateAsync(request.TaskTitle, request.TaskDescription, ct);
        return Result<AiSuggestionDto>.Success(new AiSuggestionDto(suggestion));
    }
}
