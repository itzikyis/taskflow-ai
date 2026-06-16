using FluentAssertions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Tests.Unit.Entities;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessResult()
    {
        var result = User.Create("alice@example.com", "Alice", "hashed_password");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("alice@example.com");
        result.Value.DisplayName.Should().Be("Alice");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_ReturnsFailure(string email)
    {
        var result = User.Create(email, "Alice", "hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailRequired");
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        var result = User.Create("not-an-email", "Alice", "hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailInvalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDisplayName_ReturnsFailure(string name)
    {
        var result = User.Create("alice@example.com", name, "hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.DisplayNameRequired");
    }

    [Fact]
    public void Create_NormalisesEmailToLowercase()
    {
        var result = User.Create("ALICE@EXAMPLE.COM", "Alice", "hash");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void Create_RaisesUserRegisteredEvent()
    {
        var result = User.Create("alice@example.com", "Alice", "hash");

        result.Value!.DomainEvents.Should().HaveCount(1);
    }
}
