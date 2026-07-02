namespace TaskFlow.Application.AI;

/// <summary>Represents an AI-suggested sprint plan.</summary>
/// <param name="SprintGoal">A concise statement of the sprint goal.</param>
/// <param name="SuggestedTasks">Tasks recommended for inclusion in the sprint.</param>
/// <param name="Reasoning">Explanation of the selection and goal.</param>
public sealed record SprintPlan(
    string SprintGoal,
    IReadOnlyList<SprintTaskSuggestion> SuggestedTasks,
    string Reasoning);

/// <summary>A single task suggestion within a sprint plan.</summary>
/// <param name="TaskId">The unique identifier of the task.</param>
/// <param name="Title">The task title.</param>
/// <param name="EstimatedPoints">Estimated story points (Fibonacci: 1,2,3,5,8,13).</param>
/// <param name="Justification">Reason this task was selected for the sprint.</param>
public sealed record SprintTaskSuggestion(
    Guid TaskId,
    string Title,
    int EstimatedPoints,
    string Justification);
