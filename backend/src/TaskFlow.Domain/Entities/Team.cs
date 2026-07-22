using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

/// <summary>Aggregate root representing a team of users.</summary>
public sealed class Team : AggregateRoot
{
    private readonly List<TeamMember> _members = [];

    private Team() { } // EF Core constructor

    private Team(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the team name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the optional team description.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the UTC timestamp when the team was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>Gets the members of this team.</summary>
    public IReadOnlyList<TeamMember> Members => _members.AsReadOnly();

    /// <summary>Creates a new <see cref="Team"/>.</summary>
    public static Result<Team> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Team>.Failure(TeamErrors.NameEmpty);

        if (name.Length > 100)
            return Result<Team>.Failure(TeamErrors.NameTooLong);

        return Result<Team>.Success(new Team(Guid.NewGuid(), name.Trim(), description?.Trim()));
    }

    /// <summary>Updates the team name and description.</summary>
    public void UpdateDetails(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>Renames the team to the specified <paramref name="name"/>.</summary>
    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TeamErrors.NameEmpty);

        if (name.Length > 100)
            return Result.Failure(TeamErrors.NameTooLong);

        Name = name.Trim();
        return Result.Ok;
    }

    /// <summary>Adds a user to the team with the specified role.</summary>
    public Result AddMember(Guid userId, TeamRole role)
    {
        if (_members.Any(m => m.UserId == userId))
            return Result.Failure(TeamErrors.AlreadyMember);

        _members.Add(new TeamMember(Id, userId, role));
        return Result.Ok;
    }

    /// <summary>Removes a user from the team.</summary>
    public Result RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Result.Failure(TeamErrors.MemberNotFound);

        _members.Remove(member);
        return Result.Ok;
    }

    /// <summary>Updates the role of an existing team member.</summary>
    public Result UpdateMemberRole(Guid userId, TeamRole role)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Result.Failure(TeamErrors.MemberNotFound);

        member.Role = role;
        return Result.Ok;
    }
}
