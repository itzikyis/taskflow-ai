using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.EstimateStoryPoints;

/// <summary>Handles <see cref="EstimateStoryPointsQuery"/> by delegating to the AI assistant service.</summary>
public sealed class EstimateStoryPointsQueryHandler(
    IAiAssistantService ai,
    ILogger<EstimateStoryPointsQueryHandler> logger)
    : IRequestHandler<EstimateStoryPointsQuery, Result<StoryPointEstimate>>
{
    /// <inheritdoc/>
    public async Task<Result<StoryPointEstimate>> Handle(EstimateStoryPointsQuery request, CancellationToken ct)
    {
        try
        {
            var estimate = await ai.EstimateStoryPointsAsync(request.Title, request.Description, ct);
            return Result<StoryPointEstimate>.Success(estimate);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI story point estimation failed: service is misconfigured.");
            return Result<StoryPointEstimate>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI story point estimation failed due to an unexpected error.");
            return Result<StoryPointEstimate>.Failure(AiErrors.Unavailable);
        }
    }
}
