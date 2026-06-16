using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Boards.Commands.CreateBoard;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Boards;

public sealed class CreateBoardCommandHandlerTests
{
    private readonly IBoardRepository _repo = Substitute.For<IBoardRepository>();
    private readonly CreateBoardCommandHandler _sut;

    public CreateBoardCommandHandlerTests() => _sut = new CreateBoardCommandHandler(_repo);

    [Fact]
    public async Task Handle_ValidCommand_CreatesBoardAndReturnsId()
    {
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        var result = await _sut.Handle(new CreateBoardCommand("Sprint 1", Guid.NewGuid()), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsFailureWithoutPersisting()
    {
        var result = await _sut.Handle(new CreateBoardCommand("", Guid.NewGuid()), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Board.NameRequired");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }
}
