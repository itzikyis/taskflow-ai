using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Commands.DeleteCustomField;

/// <summary>Deletes a custom field definition and all its associated task values.</summary>
/// <param name="FieldId">ID of the custom field to delete.</param>
public sealed record DeleteCustomFieldCommand(Guid FieldId) : IRequest<Result>;
