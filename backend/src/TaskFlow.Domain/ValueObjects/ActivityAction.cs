namespace TaskFlow.Domain.ValueObjects;

/// <summary>Represents the type of action recorded in an activity log entry.</summary>
public enum ActivityAction
{
    /// <summary>An entity was created.</summary>
    Created = 0,

    /// <summary>An entity was updated.</summary>
    Updated = 1,

    /// <summary>An entity was deleted.</summary>
    Deleted = 2,

    /// <summary>A task status changed.</summary>
    StatusChanged = 3,

    /// <summary>A task was assigned to a user.</summary>
    Assigned = 4,

    /// <summary>A task was moved to a board column.</summary>
    MovedToColumn = 5,

    /// <summary>A comment was added.</summary>
    CommentAdded = 6,

    /// <summary>A comment was deleted.</summary>
    CommentDeleted = 7,

    /// <summary>A member was added to a team or project.</summary>
    MemberAdded = 8,

    /// <summary>A member was removed from a team or project.</summary>
    MemberRemoved = 9,

    /// <summary>A member's role was changed.</summary>
    RoleChanged = 10,
}
