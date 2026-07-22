using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.CustomFields.Commands.CreateCustomField;

/// <summary>Creates a new custom field definition for a project.</summary>
/// <param name="ProjectId">The project to add the field to.</param>
/// <param name="Name">Display name of the field.</param>
/// <param name="FieldType">One of: Text, Number, Select, Date.</param>
/// <param name="OptionsJson">JSON array of allowed values for Select fields; empty string otherwise.</param>
public sealed record CreateCustomFieldCommand(
    Guid ProjectId,
    string Name,
    string FieldType,
    string OptionsJson = "") : IRequest<Result<Guid>>;
