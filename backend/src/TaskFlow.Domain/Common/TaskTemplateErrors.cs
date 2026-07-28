namespace TaskFlow.Domain.Common;

/// <summary>Domain errors specific to the TaskTemplate aggregate.</summary>
public static class TaskTemplateErrors
{
    /// <summary>Raised when the project id is empty.</summary>
    public static readonly Error ProjectIdRequired =
        new("TaskTemplate.ProjectIdRequired", "Project id is required.");

    /// <summary>Raised when the template name is missing.</summary>
    public static readonly Error NameRequired =
        new("TaskTemplate.NameRequired", "Template name is required.");

    /// <summary>Raised when the template name exceeds 200 characters.</summary>
    public static readonly Error NameTooLong =
        new("TaskTemplate.NameTooLong", "Template name must not exceed 200 characters.");

    /// <summary>Raised when the default title is missing.</summary>
    public static readonly Error DefaultTitleRequired =
        new("TaskTemplate.DefaultTitleRequired", "Default title is required.");

    /// <summary>Raised when the default title exceeds 200 characters.</summary>
    public static readonly Error DefaultTitleTooLong =
        new("TaskTemplate.DefaultTitleTooLong", "Default title must not exceed 200 characters.");

    /// <summary>Raised when the estimated hours value is negative.</summary>
    public static readonly Error EstimatedHoursNegative =
        new("TaskTemplate.EstimatedHoursNegative", "Estimated hours must not be negative.");

    /// <summary>Raised when a template cannot be found by id.</summary>
    public static readonly Error NotFound =
        new("TaskTemplate.NotFound", "The requested task template was not found.");
}
