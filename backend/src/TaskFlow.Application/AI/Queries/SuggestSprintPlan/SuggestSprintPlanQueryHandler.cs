using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestSprintPlan;

/// <summary>Handles <see cref="SuggestSprintPlanQuery"/> by delegating to the AI assistant service.</summary>
public sealed class SuggestSprintPlanQueryHandler(
    IAiAssistantService ai,
    ILogger<SuggestSprintPlanQueryHandler> logger)
    : IRequestHandler<SuggestSprintPlanQuery, Result<SprintPlan>>
{
    /// <inheritdoc/>
    public async Task<Result<SprintPlan>> Handle(SuggestSprintPlanQuery request, CancellationToken ct)
    {
        try
        {
            var backlog = request.Backlog.Select(t => (t.Id, t.Title, t.Description, t.Priority, t.Status));
            var plan = await ai.SuggestSprintPlanAsync(backlog, request.SprintCapacity, ct);
            return Result<SprintPlan>.Success(plan);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI sprint planning failed: service is misconfigured.");
            return Result<SprintPlan>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI sprint planning failed due to an unexpected error.");
            return Result<SprintPlan>.Failure(AiErrors.Unavailable);
        }
    }
}
