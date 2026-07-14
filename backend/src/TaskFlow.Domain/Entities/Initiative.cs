using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>Lifecycle status of an initiative.</summary>
public enum InitiativeStatus
{
    Proposed = 0,
    Active = 1,
    Completed = 2,
    Canceled = 3,
}

/// <summary>Priority of an initiative.</summary>
public enum InitiativePriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3,
}

/// <summary>
/// Cross-project initiative grouping related projects toward a shared goal (e.g. a quarterly OKR).
/// Stores project references by ID only to preserve aggregate boundaries.
/// </summary>
public sealed class Initiative : AggregateRoot
{
    private List<Guid> _projectIds = [];

    private Initiative() { } // EF Core

    // EF Core backing property for persisting project IDs as a pipe-delimited string.
    public string ProjectIdsRaw
    {
        get => string.Join('|', _projectIds);
        set => _projectIds = string.IsNullOrEmpty(value)
            ? []
            : [.. value.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse)];
    }

    /// <summary>Short name of the initiative.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional description / goal statement.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public InitiativeStatus Status { get; private set; }

    /// <summary>Priority relative to other initiatives.</summary>
    public InitiativePriority Priority { get; private set; }

    /// <summary>Comma-separated labels (e.g. "Q3,Backend,Growth").</summary>
    public string Labels { get; private set; } = string.Empty;

    /// <summary>Planned start date.</summary>
    public DateTime? StartDate { get; private set; }

    /// <summary>Planned target date.</summary>
    public DateTime? TargetDate { get; private set; }

    /// <summary>User who created this initiative.</summary>
    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>IDs of projects linked to this initiative.</summary>
    public IReadOnlyList<Guid> ProjectIds => _projectIds.AsReadOnly();

    /// <summary>Creates a new initiative.</summary>
    public static Initiative Create(
        string name,
        string description,
        InitiativePriority priority,
        string labels,
        DateTime? startDate,
        DateTime? targetDate,
        Guid createdByUserId)
    {
        var now = DateTime.UtcNow;
        return new Initiative
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            Status = InitiativeStatus.Proposed,
            Priority = priority,
            Labels = labels.Trim(),
            StartDate = startDate,
            TargetDate = targetDate,
            CreatedByUserId = createdByUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>Updates lifecycle status.</summary>
    public void UpdateStatus(InitiativeStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Updates editable fields.</summary>
    public void Update(string name, string description, InitiativePriority priority, string labels, DateTime? startDate, DateTime? targetDate)
    {
        Name = name.Trim();
        Description = description.Trim();
        Priority = priority;
        Labels = labels.Trim();
        StartDate = startDate;
        TargetDate = targetDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Links a project to this initiative.</summary>
    public Result AddProject(Guid projectId)
    {
        if (_projectIds.Contains(projectId))
            return Result.Failure(new Error("Initiative.DuplicateProject", "Project is already linked to this initiative."));
        _projectIds.Add(projectId);
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }

    /// <summary>Removes a project from this initiative.</summary>
    public Result RemoveProject(Guid projectId)
    {
        if (!_projectIds.Remove(projectId))
            return Result.Failure(new Error("Initiative.ProjectNotFound", "Project is not linked to this initiative."));
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }
}
