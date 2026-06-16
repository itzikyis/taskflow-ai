using FluentAssertions;
using Xunit;
using NSubstitute;
using TaskFlow.Application.Comments.Commands.AddComment;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tests.Unit.Comments;

/// <summary>Unit tests for <see cref="AddCommentCommandHandler"/>.</summary>
public sealed class AddCommentCommandHandlerTests
{
    private readonly ICommentRepository _repo = Substitute.For<ICommentRepository>();
    private readonly AddCommentCommandHandler _sut;

    public AddCommentCommandHandlerTests() => _sut = new AddCommentCommandHandler(_repo);

    [Fact]
    public async Task Handle_ValidCommand_AddsCommentAndReturnsId()
    {
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(new AddCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Nice!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyContent_ReturnsFailureWithoutPersisting()
    {
        var result = await _sut.Handle(new AddCommentCommand(Guid.NewGuid(), Guid.NewGuid(), ""), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ContentRequired");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }
}
