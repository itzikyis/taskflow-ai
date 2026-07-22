using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Commands.SetCustomFieldValue;

/// <summary>Sets (or updates) the value of a custom field on a task.</summary>
/// <param name="TaskId">ID of the task to update.</param>
/// <param name="CustomFieldId">ID of the custom field definition.</param>
/// <param name="Value">String representation of the value to store.</param>
public sealed record SetCustomFieldValueCommand(
    Guid TaskId,
    Guid CustomFieldId,
    string Value) : IRequest<Result>;
