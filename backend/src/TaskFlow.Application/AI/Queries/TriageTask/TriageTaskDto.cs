namespace TaskFlow.Application.AI.Queries.TriageTask;

/// <summary>
/// Result of an AI triage operation for a newly created task.
/// Contains suggested assignee, priority, duplicate flag, and the AI's reasoning.
/// </summary>
public record TriageTaskDto(
    /// <summary>The display name of the suggested assignee, or null if no suitable member was found.</summary>
    string? SuggestedAssigneeName,
    /// <summary>The ID of the suggested assignee, or null if no suitable member was found.</summary>
    Guid? SuggestedAssigneeId,
    /// <summary>The suggested priority level: "Low", "Medium", "High", or "Urgent". Null when the AI cannot determine it.</summary>
    string? SuggestedPriority,
    /// <summary>True when the task appears to duplicate an existing task in the project.</summary>
    bool IsPossibleDuplicate,
    /// <summary>Explanation of why the task is considered a duplicate, or null when it is not.</summary>
    string? DuplicateReason,
    /// <summary>The AI's overall reasoning behind all suggestions.</summary>
    string Reasoning);
