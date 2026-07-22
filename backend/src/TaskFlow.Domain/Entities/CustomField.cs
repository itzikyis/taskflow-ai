using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A custom metadata field defined per project (e.g. "Environment", "Customer", "Story Points").
/// Aggregate root — owns its definition; values are stored on <see cref="CustomFieldValue"/>.
/// </summary>
public sealed class CustomField : AggregateRoot
{
    private CustomField() { } // EF Core

    /// <summary>Gets the ID of the project this field belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the display name of the field (e.g. "Environment").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the data type of the field: "Text", "Number", "Select", or "Date".</summary>
    public string FieldType { get; private set; } = string.Empty;

    /// <summary>Gets the JSON array of allowed options for Select fields, or an empty string for other types.</summary>
    public string OptionsJson { get; private set; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this field was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new <see cref="CustomField"/> for the given project.
    /// </summary>
    /// <param name="projectId">The project this field belongs to.</param>
    /// <param name="name">Display name (non-empty, max 100 characters).</param>
    /// <param name="fieldType">One of: Text, Number, Select, Date.</param>
    /// <param name="optionsJson">JSON array of options for Select type; empty string otherwise.</param>
    /// <returns>A <see cref="Result{CustomField}"/> containing the new field, or an error.</returns>
    public static Result<CustomField> Create(
        Guid projectId,
        string name,
        string fieldType,
        string optionsJson = "")
    {
        if (projectId == Guid.Empty)
            return Result<CustomField>.Failure(CustomFieldErrors.ProjectIdRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result<CustomField>.Failure(CustomFieldErrors.NameRequired);

        if (name.Trim().Length > 100)
            return Result<CustomField>.Failure(CustomFieldErrors.NameTooLong);

        var validTypes = new[] { "Text", "Number", "Select", "Date" };
        if (!validTypes.Contains(fieldType))
            return Result<CustomField>.Failure(CustomFieldErrors.InvalidFieldType);

        return Result<CustomField>.Success(new CustomField
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name.Trim(),
            FieldType = fieldType,
            OptionsJson = optionsJson ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
        });
    }
}
