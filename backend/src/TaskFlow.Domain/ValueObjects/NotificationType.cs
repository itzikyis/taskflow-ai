namespace TaskFlow.Domain.ValueObjects;

/// <summary>Classifies the kind of notification sent to a user.</summary>
public enum NotificationType
{
    /// <summary>A task was assigned to the user.</summary>
    TaskAssigned = 0,

    /// <summary>A task the user is involved in changed status.</summary>
    TaskStatusChanged = 1,

    /// <summary>A comment was added to a task the user is involved in.</summary>
    CommentAdded = 2,

    /// <summary>A task's due date is approaching.</summary>
    DueDateApproaching = 3,

    /// <summary>The user was added to a team.</summary>
    TeamMemberAdded = 4,

    /// <summary>A general-purpose notification.</summary>
    General = 5,
}
