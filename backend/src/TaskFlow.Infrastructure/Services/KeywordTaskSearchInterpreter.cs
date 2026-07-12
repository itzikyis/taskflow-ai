using System.Text.RegularExpressions;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Search;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Deterministic natural-language interpreter that maps common English phrases to
/// a <see cref="TaskSearchFilter"/>. No external dependency; a Claude-backed
/// translator can replace it behind <see cref="ITaskSearchInterpreter"/> later.
/// </summary>
public sealed partial class KeywordTaskSearchInterpreter : ITaskSearchInterpreter
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "to", "of", "in", "on", "for", "with", "task",
        "tasks", "that", "this", "flow", "about", "regarding", "related", "show", "me",
        "my", "find", "all", "any", "still",
    };

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    /// <inheritdoc/>
    public TaskSearchFilter Interpret(string query)
    {
        var q = (query ?? string.Empty).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(q))
            return TaskSearchFilter.Empty;

        TaskItemStatus? status =
            q.Contains("in progress") || q.Contains("in-progress") || q.Contains("wip") ? TaskItemStatus.InProgress
            : q.Contains("to do") || q.Contains("todo") || q.Contains("backlog") ? TaskItemStatus.Todo
            : q.Contains("done") || q.Contains("completed") || q.Contains("complete")
              || q.Contains("finished") || q.Contains("closed") ? TaskItemStatus.Done
            : null;

        var openOnly = status is null &&
            (q.Contains("open") || q.Contains("not done") || q.Contains("unfinished") || q.Contains("outstanding"));

        TaskPriority? priority =
            q.Contains("critical") || q.Contains("urgent") ? TaskPriority.Critical
            : q.Contains("high") ? TaskPriority.High
            : q.Contains("medium") ? TaskPriority.Medium
            : q.Contains("low") ? TaskPriority.Low
            : null;

        var overdue = q.Contains("overdue") || q.Contains("past due") || q.Contains("late");

        var mineOnly = q.Contains("assigned to me") || q.Contains(" mine") || q.Contains("my task")
            || q.Contains("i created") || q.Contains("created by me") || q.Contains("i own")
            || q.StartsWith("my ") || q.Contains(" my ");

        var keywords = ExtractKeywords(q);

        return new TaskSearchFilter(status, openOnly, priority, overdue, mineOnly, keywords);
    }

    private static IReadOnlyList<string> ExtractKeywords(string q)
    {
        // Prefer explicit "about/regarding/related to X" phrasing; otherwise no
        // free-text keyword constraint (avoids over-filtering structured queries).
        var markers = new[] { "about ", "regarding ", "related to ", "mentioning " };
        foreach (var marker in markers)
        {
            var idx = q.IndexOf(marker, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var tail = q[(idx + marker.Length)..];
                return NonAlphanumeric().Split(tail)
                    .Where(t => t.Length >= 3 && !StopWords.Contains(t))
                    .Distinct()
                    .ToList();
            }
        }

        return [];
    }

    /// <inheritdoc/>
    public string Describe(TaskSearchFilter filter)
    {
        var parts = new List<string>();

        if (filter.Status is { } s) parts.Add($"{Humanize(s)} tasks");
        else if (filter.OpenOnly) parts.Add("Open tasks");
        else parts.Add("All tasks");

        if (filter.Priority is { } p) parts.Add($"{p} priority");
        if (filter.MineOnly) parts.Add("assigned to you");
        if (filter.Overdue) parts.Add("overdue");
        if (filter.Keywords.Count > 0) parts.Add($"matching “{string.Join(", ", filter.Keywords)}”");

        return string.Join(" · ", parts);
    }

    private static string Humanize(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Todo => "To-do",
        TaskItemStatus.InProgress => "In-progress",
        TaskItemStatus.Done => "Done",
        _ => status.ToString(),
    };
}
