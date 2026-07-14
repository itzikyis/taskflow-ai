namespace TaskFlow.Application.AI;

/// <summary>A single draft task proposed from meeting notes.</summary>
public sealed record MeetingActionItem(
    string Title,
    string Description,
    string Priority,
    string? SuggestedAssignee,
    string? SuggestedDueDate);

/// <summary>Result of AI meeting notes analysis.</summary>
public sealed record MeetingNotesResult(
    string Summary,
    IReadOnlyList<string> KeyDecisions,
    IReadOnlyList<MeetingActionItem> ActionItems);
