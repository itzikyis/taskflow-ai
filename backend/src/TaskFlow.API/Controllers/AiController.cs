using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.AI.Queries.AnalyzeMeetingNotes;
using TaskFlow.Application.AI.Queries.AssessSprintRisk;
using TaskFlow.Application.AI.Queries.EstimateStoryPoints;
using TaskFlow.Application.AI.Queries.SuggestDueDate;
using TaskFlow.Application.AI.Queries.GenerateReleaseNotes;
using TaskFlow.Application.AI.Queries.GenerateRetrospective;
using TaskFlow.Application.AI.Queries.SuggestSprintPlan;
using TaskFlow.Application.AI.Queries.SuggestTaskBreakdown;
using TaskFlow.Application.AI.Queries.SuggestTaskDescription;
using TaskFlow.Application.AI.Queries.SummarizeComments;

namespace TaskFlow.API.Controllers;

/// <summary>AI-assisted features for the TaskFlow application.</summary>
[ApiController]
[Route("api/ai")]
public sealed class AiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Maps an AI query failure to the correct HTTP status: genuine AI
    /// outages/misconfiguration become 503, while everything else (client-side
    /// validation) becomes 400.
    /// </summary>
    private IActionResult MapFailure(TaskFlow.Domain.Common.Error error) =>
        error.Code is "AI.Unavailable" or "AI.NotConfigured"
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, error)
            : BadRequest(error);

    /// <summary>Suggests a description for a task given its title.</summary>
    [HttpPost("suggest-description")]
    [ProducesResponseType(typeof(AiSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SuggestDescription([FromBody] SuggestDescriptionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuggestTaskDescriptionQuery(request.TaskTitle), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Suggests a due date for a task.</summary>
    [HttpPost("suggest-due-date")]
    [ProducesResponseType(typeof(AiSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SuggestDueDate([FromBody] SuggestDueDateRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuggestDueDateQuery(request.TaskTitle, request.TaskDescription), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Summarizes a list of task comments.</summary>
    [HttpPost("summarize-comments")]
    [ProducesResponseType(typeof(AiSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SummarizeComments([FromBody] SummarizeCommentsRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SummarizeCommentsQuery(request.Comments), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Suggests an AI-generated sprint plan from a list of backlog tasks.</summary>
    [HttpPost("sprint-plan")]
    [ProducesResponseType(typeof(SprintPlan), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SuggestSprintPlan(
        [FromBody] SprintPlanRequest request,
        CancellationToken ct)
    {
        // Reject malformed ids explicitly rather than silently dropping the item,
        // which would otherwise surface as a misleading "empty backlog" error.
        var invalidId = request.Backlog.FirstOrDefault(t => !Guid.TryParse(t.Id, out _));
        if (invalidId is not null)
        {
            return BadRequest(new TaskFlow.Domain.Common.Error(
                "Validation.Failed",
                $"Backlog task id '{invalidId.Id}' is not a valid GUID."));
        }

        var backlog = request.Backlog
            .Select(t => new TaskSummary(Guid.Parse(t.Id), t.Title, t.Description, t.Priority, t.Status))
            .ToList();

        var result = await mediator.Send(new SuggestSprintPlanQuery(backlog, request.SprintCapacity), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Generates AI release notes from a list of completed tasks.</summary>
    [HttpPost("release-notes")]
    [ProducesResponseType(typeof(ReleaseNotes), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GenerateReleaseNotes(
        [FromBody] GenerateReleaseNotesRequest request,
        CancellationToken ct)
    {
        var completedTasks = request.CompletedTasks
            .Select(t => new CompletedTaskSummary(t.Title, t.Description, t.Priority))
            .ToList();

        var result = await mediator.Send(new GenerateReleaseNotesQuery(request.Version, completedTasks), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Estimates story points for a task using the Fibonacci scale.</summary>
    [HttpPost("story-points")]
    [ProducesResponseType(typeof(StoryPointEstimate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> EstimateStoryPoints(
        [FromBody] EstimateStoryPointsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new EstimateStoryPointsQuery(request.Title, request.Description),
            cancellationToken);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Suggests a set of subtasks that break a larger task down into actionable work.</summary>
    [HttpPost("task-breakdown")]
    [ProducesResponseType(typeof(IReadOnlyList<SubtaskSuggestion>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> TaskBreakdown([FromBody] TaskBreakdownRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuggestTaskBreakdownQuery(request.Title, request.Description), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Generates a sprint retrospective draft from completed and incomplete tasks.</summary>
    [HttpPost("retrospective")]
    [ProducesResponseType(typeof(SprintRetrospective), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Retrospective([FromBody] RetrospectiveRequest request, CancellationToken ct)
    {
        var completed = request.Completed.Select(t => new RetroTaskSummary(t.Title, t.Description, t.Priority)).ToList();
        var incomplete = (request.Incomplete ?? [])
            .Select(t => new RetroTaskSummary(t.Title, t.Description, t.Priority)).ToList();

        var result = await mediator.Send(new GenerateSprintRetrospectiveQuery(completed, incomplete), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Performs an AI risk assessment on the supplied tasks.</summary>
    [HttpPost("risk-assessment")]
    [ProducesResponseType(typeof(SprintRiskAssessment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AssessRisk([FromBody] RiskAssessmentRequest request, CancellationToken ct)
    {
        var inputs = request.Tasks.Select(t => new RiskTaskInput(
            Guid.TryParse(t.Id, out var g) ? g : Guid.Empty,
            t.Title,
            t.Status,
            t.Priority,
            t.CreatedAt,
            t.DueDate,
            t.UpdatedAt,
            t.OpenBlockerCount)).ToList();

        var result = await mediator.Send(new AssessSprintRiskQuery(inputs), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }

    /// <summary>Analyzes meeting notes and extracts decisions and draft action-item tasks.</summary>
    [HttpPost("meeting-notes")]
    [ProducesResponseType(typeof(MeetingNotesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AnalyzeMeetingNotes([FromBody] MeetingNotesRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AnalyzeMeetingNotesQuery(request.Transcript, request.Participants ?? []), ct);
        return result.IsFailure ? MapFailure(result.Error) : Ok(result.Value);
    }
}

/// <summary>Payload for suggest-description endpoint.</summary>
public sealed record SuggestDescriptionRequest(string TaskTitle);

/// <summary>Payload for suggest-due-date endpoint.</summary>
public sealed record SuggestDueDateRequest(string TaskTitle, string? TaskDescription);

/// <summary>Payload for summarize-comments endpoint.</summary>
public sealed record SummarizeCommentsRequest(IReadOnlyList<string> Comments);

/// <summary>Payload for story-points estimation endpoint.</summary>
public sealed record EstimateStoryPointsRequest(string Title, string? Description);

/// <summary>Payload for the AI task-breakdown endpoint.</summary>
public sealed record TaskBreakdownRequest(string Title, string? Description);

/// <summary>Payload for the AI sprint-retrospective endpoint.</summary>
public sealed record RetrospectiveRequest(
    IReadOnlyList<RetroTaskInput> Completed,
    IReadOnlyList<RetroTaskInput>? Incomplete);

/// <summary>A single task entry in the retrospective request.</summary>
public sealed record RetroTaskInput(string Title, string? Description, string Priority);

/// <summary>Payload for sprint-plan endpoint.</summary>
public sealed record SprintPlanRequest(IReadOnlyList<TaskSummaryRequest> Backlog, int SprintCapacity = 40);

/// <summary>A single task in the sprint-plan request backlog.</summary>
public sealed record TaskSummaryRequest(string Id, string Title, string? Description, string Priority, string Status);

/// <summary>Payload for the release-notes generation endpoint.</summary>
public sealed record GenerateReleaseNotesRequest(string Version, IReadOnlyList<CompletedTaskSummaryRequest> CompletedTasks);

/// <summary>A single completed task entry in the release-notes request.</summary>
public sealed record CompletedTaskSummaryRequest(string Title, string? Description, string Priority);

/// <summary>Payload for the risk-assessment endpoint.</summary>
public sealed record RiskAssessmentRequest(IReadOnlyList<RiskTaskRequest> Tasks);

/// <summary>A single task snapshot for risk assessment.</summary>
public sealed record RiskTaskRequest(
    string Id,
    string Title,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? UpdatedAt,
    int OpenBlockerCount);

/// <summary>Payload for the meeting-notes analysis endpoint.</summary>
public sealed record MeetingNotesRequest(string Transcript, IReadOnlyList<string>? Participants);
