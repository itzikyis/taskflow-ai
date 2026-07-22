namespace TaskFlow.Application.CustomFields.Dtos;

/// <summary>Read model for a custom field definition.</summary>
/// <param name="Id">Unique identifier of the field.</param>
/// <param name="ProjectId">Project this field belongs to.</param>
/// <param name="Name">Display name of the field.</param>
/// <param name="FieldType">Data type: Text, Number, Select, or Date.</param>
/// <param name="OptionsJson">JSON array of allowed values (Select only); empty string otherwise.</param>
public sealed record CustomFieldDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string FieldType,
    string OptionsJson);
