namespace TaskFlow.Application.AI;

/// <summary>An AI-generated sprint retrospective draft.</summary>
public sealed record SprintRetrospective(
    string Summary,
    IReadOnlyList<string> WentWell,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> EstimateAccuracyNotes,
    IReadOnlyList<string> ActionItems);
