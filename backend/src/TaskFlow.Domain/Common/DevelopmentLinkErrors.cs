namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for task-to-code development links.</summary>
public static class DevelopmentLinkErrors
{
    /// <summary>Returned when the repository name is empty or whitespace.</summary>
    public static readonly Error RepositoryRequired = new("DevelopmentLink.RepositoryRequired", "Repository is required.");

    /// <summary>Returned when the reference title is empty or whitespace.</summary>
    public static readonly Error TitleRequired = new("DevelopmentLink.TitleRequired", "Reference title is required.");

    /// <summary>Returned when the reference URL is empty or whitespace.</summary>
    public static readonly Error UrlRequired = new("DevelopmentLink.UrlRequired", "Reference URL is required.");

    /// <summary>Returned when the requested development link does not exist.</summary>
    public static readonly Error NotFound = new("DevelopmentLink.NotFound", "Development link was not found.");
}
