namespace TaskFlow.Application.Interfaces;

/// <summary>Contract for AI-assisted features.</summary>
public interface IAiAssistantService
{
    /// <summary>Generates a suggested description for a task given its title.</summary>
    Task<string> SuggestTaskDescriptionAsync(string taskTitle, CancellationToken ct = default);

    /// <summary>Suggests a due date for a task given its title and description.</summary>
    Task<string> SuggestDueDateAsync(string taskTitle, string? taskDescription, CancellationToken ct = default);

    /// <summary>Summarizes a list of comments into a concise paragraph.</summary>
    Task<string> SummarizeCommentsAsync(IEnumerable<string> comments, CancellationToken ct = default);
}
