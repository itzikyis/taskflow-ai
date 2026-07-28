using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents a measurable Key Result associated with a <see cref="Goal"/>.
/// Not an aggregate root — owned by the Goal aggregate.
/// </summary>
public sealed class KeyResult
{
    private KeyResult() { } // EF Core constructor

    private KeyResult(Guid id, Guid goalId, string title, decimal targetValue, string unit)
    {
        Id = id;
        GoalId = goalId;
        Title = title;
        TargetValue = targetValue;
        Unit = unit;
        CurrentValue = 0;
    }

    /// <summary>Gets the unique identifier of this key result.</summary>
    public Guid Id { get; private init; } = Guid.NewGuid();

    /// <summary>Gets the identifier of the owning <see cref="Goal"/>.</summary>
    public Guid GoalId { get; private init; }

    /// <summary>Gets the title of this key result.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the target value to be reached.</summary>
    public decimal TargetValue { get; private set; }

    /// <summary>Gets the current measured value.</summary>
    public decimal CurrentValue { get; private set; }

    /// <summary>Gets the unit of measurement (e.g. %, tickets, $).</summary>
    public string Unit { get; private set; } = "%";

    /// <summary>Gets a JSON-encoded list of task IDs linked to this key result.</summary>
    public string? LinkedTaskIds { get; private set; }

    /// <summary>
    /// Creates a new <see cref="KeyResult"/>.
    /// </summary>
    public static Result<KeyResult> Create(Guid goalId, string title, decimal targetValue, string unit)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<KeyResult>.Failure(GoalErrors.KeyResultTitleRequired);

        return Result<KeyResult>.Success(
            new KeyResult(Guid.NewGuid(), goalId, title.Trim(), targetValue, unit.Trim()));
    }

    /// <summary>
    /// Calculates the progress percentage for this key result (0–100).
    /// </summary>
    public int ProgressPercent =>
        TargetValue == 0
            ? 0
            : (int)Math.Min(100, Math.Round(CurrentValue / TargetValue * 100));

    /// <summary>Updates the current measured value.</summary>
    public void UpdateProgress(decimal currentValue)
    {
        CurrentValue = currentValue < 0 ? 0 : currentValue;
    }

    /// <summary>Replaces the JSON list of linked task IDs.</summary>
    public void SetLinkedTaskIds(string? linkedTaskIds)
    {
        LinkedTaskIds = linkedTaskIds;
    }
}
