using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI.Queries.SuggestTaskDescription;
using TaskFlow.Application.Interfaces;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class SuggestTaskDescriptionQueryHandlerTests
{
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly SuggestTaskDescriptionQueryHandler _sut;

    public SuggestTaskDescriptionQueryHandlerTests() =>
        _sut = new SuggestTaskDescriptionQueryHandler(_ai, NullLogger<SuggestTaskDescriptionQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WithValidTitle_ReturnsAiSuggestion()
    {
        _ai.SuggestTaskDescriptionAsync("Fix login bug", Arg.Any<CancellationToken>())
           .Returns("Investigate and fix the authentication bug that causes login failures for users with special characters in their email.");
        var result = await _sut.Handle(new SuggestTaskDescriptionQuery("Fix login bug"), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Suggestion.Should().NotBeEmpty();
        await _ai.Received(1).SuggestTaskDescriptionAsync("Fix login bug", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyTitle_ReturnsFailureWithoutCallingAi(string title)
    {
        var result = await _sut.Handle(new SuggestTaskDescriptionQuery(title), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.TitleRequired");
        await _ai.DidNotReceive().SuggestTaskDescriptionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAiServiceThrows_ReturnsUnavailable()
    {
        _ai.SuggestTaskDescriptionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("401 Unauthorized"));

        var result = await _sut.Handle(new SuggestTaskDescriptionQuery("Set up CI"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Unavailable");
    }

    [Fact]
    public async Task Handle_WhenServiceMisconfigured_ReturnsNotConfigured()
    {
        _ai.SuggestTaskDescriptionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new InvalidOperationException("Anthropic API key not configured."));

        var result = await _sut.Handle(new SuggestTaskDescriptionQuery("Set up CI"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.NotConfigured");
    }
}
