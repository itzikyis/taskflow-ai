using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root representing a reusable task template within a project.
/// </summary>
public sealed class TaskTemplate : AggregateRoot
{
    private TaskTemplate() { } // EF Core constructor

    private TaskTemplate(
        Guid id,
        Guid projectId,
        string name,
        string defaultTitle,
        string? defaultDescription,
        string? defaultPriority,
        int? defaultEstimatedHours)
    {
        Id = id;
        ProjectId = projectId;
        Name = name;
        DefaultTitle = defaultTitle;
        DefaultDescription = defaultDescription;
        DefaultPriority = defaultPriority;
        DefaultEstimatedHours = defaultEstimatedHours;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the project this template belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the human-readable name of the template.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the default title applied to tasks created from this template.</summary>
    public string DefaultTitle { get; private set; } = string.Empty;

    /// <summary>Gets the optional default description applied to tasks created from this template.</summary>
    public string? DefaultDescription { get; private set; }

    /// <summary>Gets the optional default priority ("Low", "Medium", "High", or "Critical").</summary>
    public string? DefaultPriority { get; private set; }

    /// <summary>Gets the optional default estimated hours for tasks created from this template.</summary>
    public int? DefaultEstimatedHours { get; private set; }

    /// <summary>Gets the UTC timestamp when the template was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Creates a new <see cref="TaskTemplate"/>.
    /// </summary>
    /// <param name="projectId">The project this template belongs to.</param>
    /// <param name="name">The display name of the template.</param>
    /// <param name="defaultTitle">The default task title.</param>
    /// <param name="defaultDescription">Optional default description.</param>
    /// <param name="defaultPriority">Optional default priority string.</param>
    /// <param name="defaultEstimatedHours">Optional default hour estimate.</param>
    public static Result<TaskTemplate> Create(
        Guid projectId,
        string name,
        string defaultTitle,
        string? defaultDescription,
        string? defaultPriority,
        int? defaultEstimatedHours)
    {
        if (projectId == Guid.Empty)
            return Result<TaskTemplate>.Failure(TaskTemplateErrors.ProjectIdRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result<TaskTemplate>.Failure(TaskTemplateErrors.NameRequired);

        if (name.Length > 200)
            return Result<TaskTemplate>.Failure(TaskTemplateErrors.NameTooLong);

        if (string.IsNullOrWhiteSpace(defaultTitle))
            return Result<TaskTemplate>.Failure(TaskTemplateErrors.DefaultTitleRequired);

        if (defaultTitle.Length > 200)
            return Result<TaskTemplate>.Failure(TaskTemplateErrors.DefaultTitleTooLong);

        if (defaultEstimatedHours is < 0)
            return Result<TaskTemplate>.Failure(TaskTemplateErrors.EstimatedHoursNegative);

        var template = new TaskTemplate(
            Guid.NewGuid(),
            projectId,
            name.Trim(),
            defaultTitle.Trim(),
            defaultDescription?.Trim(),
            defaultPriority,
            defaultEstimatedHours);

        return Result<TaskTemplate>.Success(template);
    }
}
