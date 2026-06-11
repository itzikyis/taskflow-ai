namespace TaskFlow.Domain.Common;

/// <summary>Domain errors specific to the Project aggregate.</summary>
public static class ProjectErrors
{
    public static readonly Error NameRequired =
        new("Project.NameRequired", "Project name is required.");

    public static readonly Error NameTooLong =
        new("Project.NameTooLong", "Project name must not exceed 100 characters.");

    public static readonly Error NotFound =
        new("Project.NotFound", "The requested project was not found.");
}
