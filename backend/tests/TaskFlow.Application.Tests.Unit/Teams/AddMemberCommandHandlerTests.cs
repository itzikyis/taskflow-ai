using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Teams.Commands.AddMember;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Teams;

/// <summary>Unit tests for <see cref="AddMemberCommandHandler"/>.</summary>
public sealed class AddMemberCommandHandlerTests
{
    private readonly ITeamRepository _repo = Substitute.For<ITeamRepository>();
    private readonly AddMemberCommandHandler _sut;

    public AddMemberCommandHandlerTests() => _sut = new AddMemberCommandHandler(_repo);

    [Fact]
    public async Task Handle_ValidCommand_AddsMemberAndReturnsOk()
    {
        var team = Team.Create("Alpha", null).Value!;
        var userId = Guid.NewGuid();
        _repo.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var result = await _sut.Handle(
            new AddMemberCommand(team.Id, userId, TeamRole.Member),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(team);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateMember_ReturnsAlreadyMemberError()
    {
        var team = Team.Create("Beta", null).Value!;
        var userId = Guid.NewGuid();
        team.AddMember(userId, TeamRole.Member);
        _repo.GetByIdAsync(team.Id, Arg.Any<CancellationToken>()).Returns(team);

        var result = await _sut.Handle(
            new AddMemberCommand(team.Id, userId, TeamRole.Admin),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.AlreadyMember.Code);
        _repo.DidNotReceive().Update(Arg.Any<Team>());
    }

    [Fact]
    public async Task Handle_TeamNotFound_ReturnsNotFoundError()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Team?)null);

        var result = await _sut.Handle(
            new AddMemberCommand(Guid.NewGuid(), Guid.NewGuid(), TeamRole.Member),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
    }
}
