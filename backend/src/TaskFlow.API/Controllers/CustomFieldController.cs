using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.CustomFields.Commands.CreateCustomField;
using TaskFlow.Application.CustomFields.Commands.DeleteCustomField;
using TaskFlow.Application.CustomFields.Commands.SetCustomFieldValue;
using TaskFlow.Application.CustomFields.Dtos;
using TaskFlow.Application.CustomFields.Queries.GetCustomFields;
using TaskFlow.Application.CustomFields.Queries.GetTaskCustomValues;

namespace TaskFlow.API.Controllers;

/// <summary>Manages custom field definitions per project and their values on tasks.</summary>
[ApiController]
[Route("api/custom-fields")]
public sealed class CustomFieldController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all custom field definitions for a project.</summary>
    [HttpGet("projects/{projectId:guid}")]
    [ProducesResponseType(typeof(List<CustomFieldDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomFieldsQuery(projectId), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Creates a new custom field definition for a project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomFieldRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCustomFieldCommand(
            request.ProjectId,
            request.Name,
            request.FieldType,
            request.OptionsJson ?? string.Empty), ct);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(GetByProject), new { projectId = request.ProjectId }, result.Value);
    }

    /// <summary>Deletes a custom field definition.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCustomFieldCommand(id), ct);
        return result.IsFailure ? NotFound(result.Error) : NoContent();
    }

    /// <summary>Returns all custom field values set on a task.</summary>
    [HttpGet("tasks/{taskId:guid}/values")]
    [ProducesResponseType(typeof(List<CustomFieldValueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskValues(Guid taskId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTaskCustomValuesQuery(taskId), ct);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Sets the value of a custom field on a task.</summary>
    [HttpPut("tasks/{taskId:guid}/values")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetValue(Guid taskId, [FromBody] SetCustomFieldValueRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SetCustomFieldValueCommand(
            taskId,
            request.CustomFieldId,
            request.Value ?? string.Empty), ct);

        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return NoContent();
    }
}

/// <summary>Payload for creating a custom field.</summary>
public sealed record CreateCustomFieldRequest(Guid ProjectId, string Name, string FieldType, string? OptionsJson);

/// <summary>Payload for setting a custom field value on a task.</summary>
public sealed record SetCustomFieldValueRequest(Guid CustomFieldId, string? Value);
