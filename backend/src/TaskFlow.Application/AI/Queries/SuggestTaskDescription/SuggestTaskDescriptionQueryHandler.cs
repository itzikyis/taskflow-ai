using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestTaskDescription;

/// <summary>Handles <see cref="SuggestTaskDescriptionQuery"/>.</summary>
public sealed class SuggestTaskDescriptionQueryHandler(
    IAiAssistantService ai,
    ILogger<SuggestTaskDescriptionQueryHandler> logger)
    : IRequestHandler<SuggestTaskDescriptionQuery, Result<AiSuggestionDto>>
{
    public async Task<Result<AiSuggestionDto>> Handle(SuggestTaskDescriptionQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TaskTitle))
            return Result<AiSuggestionDto>.Failure(new Error("AI.TitleRequired", "Task title is required."));

        try
        {
            var suggestion = await ai.SuggestTaskDescriptionAsync(request.TaskTitle, ct);
            return Result<AiSuggestionDto>.Success(new AiSuggestionDto(suggestion));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI description suggestion failed: service is misconfigured.");
            return Result<AiSuggestionDto>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI description suggestion failed due to an unexpected error.");
            return Result<AiSuggestionDto>.Failure(AiErrors.Unavailable);
        }
    }
}
