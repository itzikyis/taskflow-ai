using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Search;

/// <summary>
/// A structured filter interpreted from a natural-language query. Any null/false
/// field means "no constraint on this dimension".
/// </summary>
public sealed record TaskSearchFilter(
    TaskItemStatus? Status,
    bool OpenOnly,
    TaskPriority? Priority,
    bool Overdue,
    bool MineOnly,
    IReadOnlyList<string> Keywords)
{
    /// <summary>An empty filter that matches everything.</summary>
    public static TaskSearchFilter Empty { get; } =
        new(null, false, null, false, false, []);
}
