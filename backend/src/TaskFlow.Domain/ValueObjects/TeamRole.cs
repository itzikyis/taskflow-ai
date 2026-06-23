namespace TaskFlow.Domain.ValueObjects;

/// <summary>Defines the role a user holds within a team.</summary>
public enum TeamRole
{
    /// <summary>Read-only access to team resources.</summary>
    Viewer = 0,

    /// <summary>Standard team member with contribution rights.</summary>
    Member = 1,

    /// <summary>Administrative access to manage team membership.</summary>
    Admin = 2,
}
