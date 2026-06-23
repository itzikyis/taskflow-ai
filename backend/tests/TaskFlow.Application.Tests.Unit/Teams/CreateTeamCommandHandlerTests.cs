using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Teams.Commands.CreateTeam;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Teams;

/// <summary>Unit tests for <see cref="CreateTeamCommandHandler"/>.</summary>
public sealed class CreateTeamCommandHandlerTests
{
    private readonly ITeamRepository _repo = Substitute.For<ITeamRepository>();
    private readonly CreateTeamCommandHandler _sut;

    public CreateTeamCommandHandlerTests() => _sut = new CreateTeamCommandHandler(_repo);

    [Fact]
    public async Task Handle_ValidCommand_CreatesTeamAndReturnsGuid()
    {
        var result = await _sut.Handle(
            new CreateTeamCommand("Engineering", "The engineering team"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<Team>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsNameEmptyError()
    {
        var result = await _sut.Handle(
            new CreateTeamCommand("", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.NameEmpty.Code);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Team>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NameTooLong_ReturnsNameTooLongError()
    {
        var result = await _sut.Handle(
            new CreateTeamCommand(new string('x', 101), null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TeamErrors.NameTooLong.Code);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Team>(), Arg.Any<CancellationToken>());
    }
}
