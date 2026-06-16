using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.AI.Dtos;
using TaskFlow.Application.AI.Queries.SuggestDueDate;
using TaskFlow.Application.AI.Queries.SuggestTaskDescription;
using TaskFlow.Application.AI.Queries.SummarizeComments;

namespace TaskFlow.API.Controllers;

/// <summary>AI-assisted features for the TaskFlow application.</summary>
[ApiController]
[Route("api/ai")]
public sealed class AiController(IMediator mediator) : ControllerBase
{
    /// <summary>Suggests a description for a task given its title.</summary>
    [HttpPost("suggest-description")]
    [ProducesResponseType(typeof(AiSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuggestDescription([FromBody] SuggestDescriptionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuggestTaskDescriptionQuery(request.TaskTitle), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Suggests a due date for a task.</summary>
    [HttpPost("suggest-due-date")]
    [ProducesResponseType(typeof(AiSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuggestDueDate([FromBody] SuggestDueDateRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuggestDueDateQuery(request.TaskTitle, request.TaskDescription), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Summarizes a list of task comments.</summary>
    [HttpPost("summarize-comments")]
    [ProducesResponseType(typeof(AiSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SummarizeComments([FromBody] SummarizeCommentsRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SummarizeCommentsQuery(request.Comments), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }
}

/// <summary>Payload for suggest-description endpoint.</summary>
public sealed record SuggestDescriptionRequest(string TaskTitle);

/// <summary>Payload for suggest-due-date endpoint.</summary>
public sealed record SuggestDueDateRequest(string TaskTitle, string? TaskDescription);

/// <summary>Payload for summarize-comments endpoint.</summary>
public sealed record SummarizeCommentsRequest(IReadOnlyList<string> Comments);
