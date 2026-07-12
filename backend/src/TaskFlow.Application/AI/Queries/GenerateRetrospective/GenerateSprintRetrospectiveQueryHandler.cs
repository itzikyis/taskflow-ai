using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GenerateRetrospective;

/// <summary>Handles <see cref="GenerateSprintRetrospectiveQuery"/>.</summary>
public sealed class GenerateSprintRetrospectiveQueryHandler(
    IAiAssistantService ai,
    ILogger<GenerateSprintRetrospectiveQueryHandler> logger)
    : IRequestHandler<GenerateSprintRetrospectiveQuery, Result<SprintRetrospective>>
{
    /// <inheritdoc/>
    public async Task<Result<SprintRetrospective>> Handle(
        GenerateSprintRetrospectiveQuery request, CancellationToken ct)
    {
        try
        {
            var completed = request.Completed.Select(t => (t.Title, t.Description, t.Priority));
            var incomplete = request.Incomplete.Select(t => (t.Title, t.Description, t.Priority));

            var retro = await ai.GenerateRetrospectiveAsync(completed, incomplete, ct);
            return Result<SprintRetrospective>.Success(retro);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI retrospective generation failed: service is misconfigured.");
            return Result<SprintRetrospective>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI retrospective generation failed due to an unexpected error.");
            return Result<SprintRetrospective>.Failure(AiErrors.Unavailable);
        }
    }
}
