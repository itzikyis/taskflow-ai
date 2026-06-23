using FluentAssertions;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Teams;

/// <summary>Unit tests for the <see cref="Team"/> aggregate.</summary>
public sealed class TeamDomainTests
{
    [Fact]
    public void Create_ValidName_ReturnsSuccessWithTeam()
    {
        var result = Team.Create("Engineering", "The engineering team");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Engineering");
        result.Value.Description.Should().Be("The engineering team");
        result.Value.Members.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyName_ReturnsNameEmptyError()
    {
        var result = Team.Create("", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.NameEmpty.Code);
    }

    [Fact]
    public void Create_WhitespaceName_ReturnsNameEmptyError()
    {
        var result = Team.Create("   ", null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.NameEmpty.Code);
    }

    [Fact]
    public void Create_NameExceeds100Chars_ReturnsNameTooLongError()
    {
        var longName = new string('x', 101);
        var result = Team.Create(longName, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.NameTooLong.Code);
    }

    [Fact]
    public void AddMember_NewUser_Succeeds()
    {
        var team = Team.Create("Alpha", null).Value!;
        var userId = Guid.NewGuid();

        var result = team.AddMember(userId, TeamRole.Member);

        result.IsSuccess.Should().BeTrue();
        team.Members.Should().ContainSingle(m => m.UserId == userId && m.Role == TeamRole.Member);
    }

    [Fact]
    public void AddMember_DuplicateUser_ReturnsAlreadyMemberError()
    {
        var team = Team.Create("Beta", null).Value!;
        var userId = Guid.NewGuid();
        team.AddMember(userId, TeamRole.Member);

        var result = team.AddMember(userId, TeamRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.AlreadyMember.Code);
    }

    [Fact]
    public void RemoveMember_ExistingMember_Succeeds()
    {
        var team = Team.Create("Gamma", null).Value!;
        var userId = Guid.NewGuid();
        team.AddMember(userId, TeamRole.Member);

        var result = team.RemoveMember(userId);

        result.IsSuccess.Should().BeTrue();
        team.Members.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_NonExistentUser_ReturnsMemberNotFoundError()
    {
        var team = Team.Create("Delta", null).Value!;

        var result = team.RemoveMember(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.MemberNotFound.Code);
    }

    [Fact]
    public void UpdateMemberRole_ExistingMember_ChangesRole()
    {
        var team = Team.Create("Epsilon", null).Value!;
        var userId = Guid.NewGuid();
        team.AddMember(userId, TeamRole.Member);

        var result = team.UpdateMemberRole(userId, TeamRole.Admin);

        result.IsSuccess.Should().BeTrue();
        team.Members.Single(m => m.UserId == userId).Role.Should().Be(TeamRole.Admin);
    }

    [Fact]
    public void UpdateMemberRole_NonExistentUser_ReturnsMemberNotFoundError()
    {
        var team = Team.Create("Zeta", null).Value!;

        var result = team.UpdateMemberRole(Guid.NewGuid(), TeamRole.Admin);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.MemberNotFound.Code);
    }
}
