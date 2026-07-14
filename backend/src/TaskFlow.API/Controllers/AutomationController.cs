using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Automations.Commands.CreateAutomationRule;
using TaskFlow.Application.Automations.Commands.DeleteAutomationRule;
using TaskFlow.Application.Automations.Commands.ToggleAutomationRule;
using TaskFlow.Application.Automations.Dtos;
using TaskFlow.Application.Automations.Queries.GetAutomationRules;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Controllers;

/// <summary>CRUD for no-code automation rules.</summary>
[ApiController]
[Route("api/automations")]
public sealed class AutomationController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all automation rules for a project.</summary>
    [HttpGet("projects/{projectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AutomationRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
    {
        var rules = await mediator.Send(new GetAutomationRulesQuery(projectId), ct);
        return Ok(rules);
    }

    /// <summary>Creates a new automation rule for a project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRuleRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<AutomationTriggerType>(request.TriggerType, out var trigger))
            return BadRequest($"Unknown trigger type '{request.TriggerType}'.");
        if (!Enum.TryParse<AutomationActionType>(request.ActionType, out var action))
            return BadRequest($"Unknown action type '{request.ActionType}'.");

        var result = await mediator.Send(new CreateAutomationRuleCommand(
            request.ProjectId, request.Name, trigger, request.TriggerValue, action, request.ActionValue), ct);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetByProject), new { projectId = request.ProjectId }, result.Value);
    }

    /// <summary>Enables or disables an automation rule.</summary>
    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Toggle(Guid id, [FromBody] ToggleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new ToggleAutomationRuleCommand(id, request.IsEnabled), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>Deletes an automation rule.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAutomationRuleCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }
}

/// <summary>Payload for creating an automation rule.</summary>
public sealed record CreateRuleRequest(
    Guid ProjectId,
    string Name,
    string TriggerType,
    string TriggerValue,
    string ActionType,
    string ActionValue);

/// <summary>Payload for toggling a rule's enabled state.</summary>
public sealed record ToggleRequest(bool IsEnabled);
