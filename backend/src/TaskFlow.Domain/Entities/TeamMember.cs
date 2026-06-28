using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

/// <summary>Represents the membership of a user in a team.</summary>
public sealed class TeamMember
{
    private TeamMember() { } // EF Core constructor

    internal TeamMember(Guid teamId, Guid userId, TeamRole role)
    {
        TeamId = teamId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the ID of the team.</summary>
    public Guid TeamId { get; private init; }

    /// <summary>Gets the ID of the user.</summary>
    public Guid UserId { get; private init; }

    /// <summary>Gets the role of the user within the team.</summary>
    public TeamRole Role { get; internal set; }

    /// <summary>Gets the UTC timestamp when the user joined the team.</summary>
    public DateTime JoinedAt { get; private init; }
}
