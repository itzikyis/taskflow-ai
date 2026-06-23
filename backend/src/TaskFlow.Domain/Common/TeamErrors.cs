namespace TaskFlow.Domain.Common;

/// <summary>Domain errors specific to the Team aggregate.</summary>
public static class TeamErrors
{
    /// <summary>The requested team was not found.</summary>
    public static readonly Error NotFound =
        new("Team.NotFound", "The requested team was not found.");

    /// <summary>Team name is required.</summary>
    public static readonly Error NameEmpty =
        new("Team.NameEmpty", "Team name is required.");

    /// <summary>Team name exceeds the maximum allowed length.</summary>
    public static readonly Error NameTooLong =
        new("Team.NameTooLong", "Team name must not exceed 100 characters.");

    /// <summary>The user is already a member of the team.</summary>
    public static readonly Error AlreadyMember =
        new("Team.AlreadyMember", "The user is already a member of this team.");

    /// <summary>The user is not a member of the team.</summary>
    public static readonly Error MemberNotFound =
        new("Team.MemberNotFound", "The user is not a member of this team.");
}
