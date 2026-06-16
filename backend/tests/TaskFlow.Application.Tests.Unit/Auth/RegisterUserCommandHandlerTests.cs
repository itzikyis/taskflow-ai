using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Auth.Commands.Register;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tests.Unit.Auth;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _hasher.Hash(Arg.Any<string>()).Returns("hashed_password");
        _sut = new RegisterUserCommandHandler(_repo, _hasher);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsId()
    {
        _repo.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new RegisterUserCommand("alice@example.com", "Alice", "Password1!");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsEmailAlreadyTakenError()
    {
        _repo.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var command = new RegisterUserCommand("alice@example.com", "Alice", "Password1!");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailAlreadyTaken");
        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsFailureWithoutPersisting()
    {
        _repo.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new RegisterUserCommand("not-an-email", "Alice", "Password1!");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
