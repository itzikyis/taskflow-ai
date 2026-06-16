namespace TaskFlow.Domain.Entities;

/// <summary>A column on a Kanban board (owned entity — not an aggregate root).</summary>
public sealed class BoardColumn
{
    private BoardColumn() { }

    internal BoardColumn(Guid id, Guid boardId, string name, int order, int? wipLimit)
    {
        Id = id;
        BoardId = boardId;
        Name = name;
        Order = order;
        WipLimit = wipLimit;
    }

    /// <summary>Gets the column id.</summary>
    public Guid Id { get; private init; } = Guid.NewGuid();

    /// <summary>Gets the owning board id.</summary>
    public Guid BoardId { get; private init; }

    /// <summary>Gets the display name.</summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>Gets the display order (0-based).</summary>
    public int Order { get; internal set; }

    /// <summary>Gets the optional WIP limit (null = no limit).</summary>
    public int? WipLimit { get; internal set; }
}
