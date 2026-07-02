using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.EstimateStoryPoints;

/// <summary>Handles <see cref="EstimateStoryPointsQuery"/> by delegating to the AI assistant service.</summary>
public sealed class EstimateStoryPointsQueryHandler(IAiAssistantService ai)
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
        catch (Exception)
        {
            return Result<StoryPointEstimate>.Failure(new Error("AI.Unavailable", "AI service unavailable."));
        }
    }
}
