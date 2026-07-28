using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root representing an Objective in an OKR (Objectives and Key Results) setup.
/// </summary>
public sealed class Goal : AggregateRoot
{
    private readonly List<KeyResult> _keyResults = [];

    private Goal() { } // EF Core constructor

    private Goal(Guid id, Guid projectId, Guid ownerId, string title, string? description, DateTime? dueDate)
    {
        Id = id;
        ProjectId = projectId;
        OwnerId = ownerId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = "OnTrack";
        ProgressPercent = 0;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the project this goal belongs to.</summary>
    public Guid ProjectId { get; private init; }

    /// <summary>Gets the ID of the user who owns this goal.</summary>
    public Guid OwnerId { get; private init; }

    /// <summary>Gets the goal title (the Objective).</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the optional description of the goal.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the status of the goal. One of: OnTrack, AtRisk, OffTrack, Completed.</summary>
    public string Status { get; private set; } = "OnTrack";

    /// <summary>Gets the overall progress percentage (0–100).</summary>
    public int ProgressPercent { get; private set; }

    /// <summary>Gets the optional due date for this goal.</summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>Gets the UTC timestamp when the goal was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the key results associated with this goal.</summary>
    public IReadOnlyList<KeyResult> KeyResults => _keyResults.AsReadOnly();

    /// <summary>
    /// Factory method for creating a new <see cref="Goal"/>.
    /// </summary>
    public static Result<Goal> Create(
        Guid projectId,
        Guid ownerId,
        string title,
        string? description,
        DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<Goal>.Failure(GoalErrors.TitleRequired);

        if (title.Length > 200)
            return Result<Goal>.Failure(GoalErrors.TitleTooLong);

        return Result<Goal>.Success(
            new Goal(Guid.NewGuid(), projectId, ownerId, title.Trim(), description?.Trim(), dueDate));
    }

    /// <summary>Updates the overall progress percentage.</summary>
    public Result UpdateProgress(int percent)
    {
        if (percent < 0 || percent > 100)
            return Result.Failure(GoalErrors.InvalidProgress);

        ProgressPercent = percent;
        return Result.Ok;
    }

    /// <summary>Sets the status of the goal.</summary>
    public void SetStatus(string status)
    {
        Status = status;
    }
}
