using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestDueDate;

/// <summary>Handles <see cref="SuggestDueDateQuery"/>.</summary>
public sealed class SuggestDueDateQueryHandler(
    IAiAssistantService ai,
    ILogger<SuggestDueDateQueryHandler> logger)
    : IRequestHandler<SuggestDueDateQuery, Result<AiSuggestionDto>>
{
    public async Task<Result<AiSuggestionDto>> Handle(SuggestDueDateQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TaskTitle))
            return Result<AiSuggestionDto>.Failure(new Error("AI.TitleRequired", "Task title is required."));

        try
        {
            var suggestion = await ai.SuggestDueDateAsync(request.TaskTitle, request.TaskDescription, ct);
            return Result<AiSuggestionDto>.Success(new AiSuggestionDto(suggestion));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI due-date suggestion failed: service is misconfigured.");
            return Result<AiSuggestionDto>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI due-date suggestion failed due to an unexpected error.");
            return Result<AiSuggestionDto>.Failure(AiErrors.Unavailable);
        }
    }
}
