using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.AI.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.AssessSprintRisk;

/// <summary>Handles <see cref="AssessSprintRiskQuery"/> by delegating to the AI assistant.</summary>
public sealed class AssessSprintRiskQueryHandler(
    IAiAssistantService ai,
    ILogger<AssessSprintRiskQueryHandler> logger)
    : IRequestHandler<AssessSprintRiskQuery, Result<SprintRiskAssessment>>
{
    /// <inheritdoc/>
    public async Task<Result<SprintRiskAssessment>> Handle(AssessSprintRiskQuery request, CancellationToken ct)
    {
        if (request.Tasks.Count == 0)
            return Result<SprintRiskAssessment>.Failure(new Error("Risk.NoTasks", "No tasks provided for risk assessment."));

        try
        {
            var result = await ai.AssessSprintRiskAsync(request.Tasks, ct);
            return Result<SprintRiskAssessment>.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "AI risk assessment failed: service is misconfigured.");
            return Result<SprintRiskAssessment>.Failure(AiErrors.NotConfigured);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI risk assessment failed due to an unexpected error.");
            return Result<SprintRiskAssessment>.Failure(AiErrors.Unavailable);
        }
    }
}
