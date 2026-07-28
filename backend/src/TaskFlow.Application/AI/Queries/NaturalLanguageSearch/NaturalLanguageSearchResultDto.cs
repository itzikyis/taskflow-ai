namespace TaskFlow.Application.AI.Queries.NaturalLanguageSearch;

/// <summary>
/// A single matching task returned from a natural-language search.
/// </summary>
/// <param name="Id">The task identifier.</param>
/// <param name="Title">The task title.</param>
/// <param name="Description">The optional task description.</param>
/// <param name="Status">The current status as a string.</param>
/// <param name="Priority">The priority as a string.</param>
/// <param name="DueDate">The optional due date.</param>
/// <param name="AssignedToUserId">The optional ID of the assigned user.</param>
public sealed record TaskSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId);

/// <summary>
/// Result returned by <see cref="NaturalLanguageSearchQuery"/>.
/// </summary>
/// <param name="Tasks">The list of tasks that matched the interpreted filter.</param>
/// <param name="Interpretation">A human-readable explanation of how the query was interpreted.</param>
public sealed record NaturalLanguageSearchResultDto(
    List<TaskSummaryDto> Tasks,
    string Interpretation);
