using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestSprintPlan;

/// <summary>Handles <see cref="SuggestSprintPlanQuery"/> by delegating to the AI assistant service.</summary>
public sealed class SuggestSprintPlanQueryHandler(IAiAssistantService ai)
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
        catch (Exception)
        {
            return Result<SprintPlan>.Failure(new Error("AI.Unavailable", "AI service unavailable."));
        }
    }
}
