using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Auth.Commands.Login;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tests.Unit.Auth;

public sealed class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _repo     = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher   = Substitute.For<IPasswordHasher>();
    private readonly IJwtService     _jwt      = Substitute.For<IJwtService>();
    private readonly LoginUserCommandHandler _sut;

    public LoginUserCommandHandlerTests()
    {
        _sut = new LoginUserCommandHandler(_repo, _hasher, _jwt);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthTokenDto()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed").Value!;
        _repo.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("Password1!", "hashed").Returns(true);
        _jwt.GenerateToken(user).Returns("signed.jwt.token");

        var result = await _sut.Handle(
            new LoginUserCommand("alice@example.com", "Password1!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("signed.jwt.token");
        result.Value.Email.Should().Be("alice@example.com");
        result.Value.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsInvalidCredentials()
    {
        _repo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.Handle(
            new LoginUserCommand("nobody@example.com", "Password1!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsInvalidCredentials()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed").Value!;
        _repo.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("WrongPass!", "hashed").Returns(false);

        var result = await _sut.Handle(
            new LoginUserCommand("alice@example.com", "WrongPass!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        _jwt.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}
