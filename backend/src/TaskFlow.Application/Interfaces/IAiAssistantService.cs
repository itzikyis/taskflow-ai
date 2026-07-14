using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.AssessSprintRisk;

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

    /// <summary>Estimates story points for a task using Fibonacci scale (1, 2, 3, 5, 8, 13).</summary>
    Task<StoryPointEstimate> EstimateStoryPointsAsync(string title, string? description, CancellationToken ct = default);

    /// <summary>Suggests a sprint plan from a backlog of tasks.</summary>
    Task<SprintPlan> SuggestSprintPlanAsync(
        IEnumerable<(Guid Id, string Title, string? Description, string Priority, string Status)> backlog,
        int sprintCapacity,
        CancellationToken ct = default);

    /// <summary>Generates release notes from a list of completed tasks.</summary>
    Task<ReleaseNotes> GenerateReleaseNotesAsync(
        string version,
        IEnumerable<(string Title, string? Description, string Priority)> completedTasks,
        CancellationToken ct = default);

    /// <summary>Suggests 3-8 subtasks that break a larger task down into actionable work.</summary>
    Task<IReadOnlyList<SubtaskSuggestion>> GenerateSubtasksAsync(
        string title,
        string? description,
        CancellationToken ct = default);

    /// <summary>Generates a sprint retrospective draft from completed and incomplete tasks.</summary>
    Task<SprintRetrospective> GenerateRetrospectiveAsync(
        IEnumerable<(string Title, string? Description, string Priority)> completed,
        IEnumerable<(string Title, string? Description, string Priority)> incomplete,
        CancellationToken ct = default);

    /// <summary>Drafts a proposed approach for a task, as if an AI agent were picking it up.</summary>
    Task<string> DraftTaskApproachAsync(string title, string? description, CancellationToken ct = default);

    /// <summary>Assesses risk level for a set of tasks and returns a sprint-wide health summary.</summary>
    Task<SprintRiskAssessment> AssessSprintRiskAsync(
        IReadOnlyList<Application.AI.Queries.AssessSprintRisk.RiskTaskInput> tasks,
        CancellationToken ct = default);

    /// <summary>Analyzes meeting notes or transcript and extracts decisions and action items.</summary>
    Task<MeetingNotesResult> AnalyzeMeetingNotesAsync(
        string transcript,
        IReadOnlyList<string> participantNames,
        CancellationToken ct = default);
}
