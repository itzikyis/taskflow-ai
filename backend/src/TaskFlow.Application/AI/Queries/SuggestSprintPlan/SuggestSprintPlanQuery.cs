using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.SuggestSprintPlan;

/// <summary>Query to generate an AI-suggested sprint plan from a task backlog.</summary>
/// <param name="Backlog">The list of tasks to consider for the sprint.</param>
/// <param name="SprintCapacity">Total story point capacity for the sprint.</param>
public sealed record SuggestSprintPlanQuery(
    IReadOnlyList<TaskSummary> Backlog,
    int SprintCapacity = 40) : IRequest<Result<SprintPlan>>;

/// <summary>Lightweight task summary used as input to sprint planning.</summary>
/// <param name="Id">Unique identifier of the task.</param>
/// <param name="Title">Task title.</param>
/// <param name="Description">Optional task description.</param>
/// <param name="Priority">Task priority (e.g. Low, Medium, High, Critical).</param>
/// <param name="Status">Current task status (e.g. Todo, InProgress, Done).</param>
public sealed record TaskSummary(Guid Id, string Title, string? Description, string Priority, string Status);
