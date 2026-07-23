namespace TaskFlow.Domain.Entities;

/// <summary>
/// Stores the value of a <see cref="CustomField"/> for a specific task.
/// Not an aggregate root — it is managed through <see cref="CustomField"/> queries
/// and persisted via <see cref="TaskFlow.Application.Interfaces.ICustomFieldRepository"/>.
/// </summary>
public sealed class CustomFieldValue
{
    private CustomFieldValue() { } // EF Core

    /// <summary>Gets the unique identifier of this value record.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the task this value belongs to.</summary>
    public Guid TaskId { get; private set; }

    /// <summary>Gets the custom field this value is for.</summary>
    public Guid CustomFieldId { get; private set; }

    /// <summary>Gets the stored value as a string (serialised for all field types).</summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="CustomFieldValue"/> associating a task with a custom field.
    /// </summary>
    /// <param name="taskId">ID of the task.</param>
    /// <param name="customFieldId">ID of the custom field definition.</param>
    /// <param name="value">String representation of the value.</param>
    /// <returns>A new <see cref="CustomFieldValue"/> instance.</returns>
    public static CustomFieldValue Create(Guid taskId, Guid customFieldId, string value) =>
        new()
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            CustomFieldId = customFieldId,
            Value = value ?? string.Empty,
        };

    /// <summary>Updates the stored value.</summary>
    /// <param name="value">New string value.</param>
    public void SetValue(string value) =>
        Value = value ?? string.Empty;
}
