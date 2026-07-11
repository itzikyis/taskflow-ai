using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestTaskBreakdown;

/// <summary>Handles <see cref="SuggestTaskBreakdownQuery"/> by delegating to the AI service.</summary>
public sealed class SuggestTaskBreakdownQueryHandler(
    IAiAssistantService ai,
    ILogger<SuggestTaskBreakdownQueryHandler> logger)
    : IRequestHandler<SuggestTaskBreakdownQuery, Result<IReadOnlyList<SubtaskSuggestion>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<SubtaskSuggestion>>> Handle(
        SuggestTaskBreakdownQuery request, CancellationToken ct)
    {
        try
        {
            var subtasks = await ai.GenerateSubtasksAsync(request.Title, request.Description, ct);
            return Result<IReadOnlyList<SubtaskSuggestion>>.Success(subtasks);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI task breakdown failed: service is misconfigured.");
            return Result<IReadOnlyList<SubtaskSuggestion>>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI task breakdown failed due to an unexpected error.");
            return Result<IReadOnlyList<SubtaskSuggestion>>.Failure(AiErrors.Unavailable);
        }
    }
}
