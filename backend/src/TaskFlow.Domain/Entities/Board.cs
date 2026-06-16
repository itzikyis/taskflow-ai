using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

/// <summary>Aggregate root representing a Kanban board belonging to a project.</summary>
public sealed class Board : AggregateRoot
{
    private readonly List<BoardColumn> _columns = [];

    private Board() { }

    private Board(Guid id, string name, Guid projectId)
    {
        Id = id;
        Name = name;
        ProjectId = projectId;
        CreatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BoardCreatedEvent(id, name, projectId));
    }

    /// <summary>Gets the board name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the project this board belongs to.</summary>
    public Guid ProjectId { get; private init; }

    /// <summary>Gets the creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the last update timestamp (UTC), or null if never updated.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Gets the board's columns.</summary>
    public IReadOnlyList<BoardColumn> Columns => _columns.AsReadOnly();

    /// <summary>Creates a new board for the given project.</summary>
    public static Result<Board> Create(string name, Guid projectId)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result<Board>.Failure(BoardErrors.NameRequired);
        if (name.Length > 100) return Result<Board>.Failure(BoardErrors.NameTooLong);
        return Result<Board>.Success(new Board(Guid.NewGuid(), name.Trim(), projectId));
    }

    /// <summary>Updates the board name.</summary>
    public Result Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure(BoardErrors.NameRequired);
        if (name.Length > 100) return Result.Failure(BoardErrors.NameTooLong);
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }

    /// <summary>Adds a column to the board.</summary>
    public Result<BoardColumn> AddColumn(string name, int order, int? wipLimit = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result<BoardColumn>.Failure(BoardErrors.ColumnNameRequired);
        if (name.Length > 100) return Result<BoardColumn>.Failure(BoardErrors.ColumnNameTooLong);
        if (_columns.Any(c => c.Order == order)) return Result<BoardColumn>.Failure(BoardErrors.DuplicateColumnOrder);
        var col = new BoardColumn(Guid.NewGuid(), Id, name.Trim(), order, wipLimit);
        _columns.Add(col);
        UpdatedAt = DateTime.UtcNow;
        return Result<BoardColumn>.Success(col);
    }

    /// <summary>Removes a column from the board.</summary>
    public Result RemoveColumn(Guid columnId)
    {
        var col = _columns.FirstOrDefault(c => c.Id == columnId);
        if (col is null) return Result.Failure(BoardErrors.ColumnNotFound);
        _columns.Remove(col);
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok;
    }
}
