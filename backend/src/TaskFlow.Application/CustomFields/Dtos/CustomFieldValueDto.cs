namespace TaskFlow.Application.CustomFields.Dtos;

/// <summary>Read model for a custom field value set on a task.</summary>
/// <param name="CustomFieldId">ID of the custom field definition.</param>
/// <param name="FieldName">Display name of the field.</param>
/// <param name="FieldType">Data type: Text, Number, Select, or Date.</param>
/// <param name="Value">Current value stored for this field on the task.</param>
public sealed record CustomFieldValueDto(
    Guid CustomFieldId,
    string FieldName,
    string FieldType,
    string Value);
