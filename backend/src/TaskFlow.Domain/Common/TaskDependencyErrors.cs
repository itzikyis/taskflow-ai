namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for task dependencies.</summary>
public static class TaskDependencyErrors
{
    /// <summary>Returned when a task is made to depend on itself.</summary>
    public static readonly Error SelfDependency =
        new("TaskDependency.Self", "A task cannot depend on itself.");

    /// <summary>Returned when the same dependency already exists.</summary>
    public static readonly Error Duplicate =
        new("TaskDependency.Duplicate", "This dependency already exists.");

    /// <summary>Returned when adding the dependency would create a cycle.</summary>
    public static readonly Error Cycle =
        new("TaskDependency.Cycle", "This dependency would create a circular chain.");

    /// <summary>Returned when the requested dependency does not exist.</summary>
    public static readonly Error NotFound =
        new("TaskDependency.NotFound", "Dependency was not found.");
}
