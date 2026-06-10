using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root representing a project that groups tasks together.
/// </summary>
public sealed class Project : AggregateRoot
{
    private Project() { } // EF Core constructor

    private Project(Guid id, string name, string? description, Guid ownerId)
    {
        Id = id;
        Name = name;
        Description = description;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProjectCreatedEvent(id, name, ownerId));
    }

    /// <summary>Gets the project name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the optional project description.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the ID of the user who owns this project.</summary>
    public Guid OwnerId { get; private init; }

    /// <summary>Gets the UTC timestamp when the project was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the UTC timestamp of the last update.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Creates a new <see cref="Project"/>.</summary>
    public static Result<Project> Create(string name, string? description, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Project>.Failure(ProjectErrors.NameRequired);

        if (name.Length > 100)
            return Result<Project>.Failure(ProjectErrors.NameTooLong);

        return Result<Project>.Success(
            new Project(Guid.NewGuid(), name.Trim(), description?.Trim(), ownerId));
    }

    /// <summary>Updates the project name and description.</summary>
    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ProjectErrors.NameRequired);

        if (name.Length > 100)
            return Result.Failure(ProjectErrors.NameTooLong);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }
}
