using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.NaturalLanguageSearch;

/// <summary>
/// Handles <see cref="NaturalLanguageSearchQuery"/> by interpreting the free-text query via
/// <see cref="ITaskSearchInterpreter"/> and then filtering tasks from <see cref="ITaskRepository"/>.
/// </summary>
public sealed class NaturalLanguageSearchQueryHandler(
    ITaskSearchInterpreter interpreter,
    ITaskRepository taskRepository)
    : IRequestHandler<NaturalLanguageSearchQuery, Result<NaturalLanguageSearchResultDto>>
{
    /// <inheritdoc/>
    public async Task<Result<NaturalLanguageSearchResultDto>> Handle(
        NaturalLanguageSearchQuery request,
        CancellationToken cancellationToken)
    {
        var filter = interpreter.Interpret(request.Query);
        var interpretation = interpreter.Describe(filter);

        var allTasks = await taskRepository.GetAllAsync(cancellationToken: cancellationToken);

        var now = DateTime.UtcNow;

        var matched = allTasks
            .Where(t =>
                // Status filter
                (filter.Status is null || t.Status == filter.Status) &&
                // Open-only filter (any non-Done status)
                (!filter.OpenOnly || t.Status != TaskFlow.Domain.ValueObjects.TaskItemStatus.Done) &&
                // Priority filter
                (filter.Priority is null || t.Priority == filter.Priority) &&
                // Overdue filter
                (!filter.Overdue || (t.DueDate.HasValue && t.DueDate.Value < now)) &&
                // Mine-only filter
                (!filter.MineOnly || t.AssignedToUserId == request.UserId || t.CreatedByUserId == request.UserId) &&
                // Keyword filter — all keywords must appear in title or description
                (filter.Keywords.Count == 0 || filter.Keywords.All(kw =>
                    t.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(kw, StringComparison.OrdinalIgnoreCase))))
            )
            .Select(t => new TaskSummaryDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.DueDate,
                t.AssignedToUserId))
            .ToList();

        return Result<NaturalLanguageSearchResultDto>.Success(
            new NaturalLanguageSearchResultDto(matched, interpretation));
    }
}
