using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.EstimateStoryPoints;

/// <summary>Query that requests an AI-based story point estimate for a task.</summary>
/// <param name="Title">Title of the task.</param>
/// <param name="Description">Optional task description for additional context.</param>
public sealed record EstimateStoryPointsQuery(string Title, string? Description)
    : IRequest<Result<StoryPointEstimate>>;
