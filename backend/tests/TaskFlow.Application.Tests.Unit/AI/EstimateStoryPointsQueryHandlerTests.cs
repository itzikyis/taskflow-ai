using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.EstimateStoryPoints;
using TaskFlow.Application.Interfaces;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class EstimateStoryPointsQueryHandlerTests
{
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly EstimateStoryPointsQueryHandler _sut;

    public EstimateStoryPointsQueryHandlerTests() =>
        _sut = new EstimateStoryPointsQueryHandler(_ai, NullLogger<EstimateStoryPointsQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenAiReturnsEstimate_ReturnsSuccessWithCorrectPoints()
    {
        var estimate = new StoryPointEstimate(5, "This task involves moderate complexity.");
        _ai.EstimateStoryPointsAsync("Implement login page", "Add OAuth2 login flow", Arg.Any<CancellationToken>())
           .Returns(estimate);

        var result = await _sut.Handle(
            new EstimateStoryPointsQuery("Implement login page", "Add OAuth2 login flow"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Points.Should().Be(5);
        result.Value.Reasoning.Should().Be("This task involves moderate complexity.");
        await _ai.Received(1).EstimateStoryPointsAsync(
            "Implement login page", "Add OAuth2 login flow", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAiServiceThrows_ReturnsFailureResult()
    {
        _ai.EstimateStoryPointsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("AI service unreachable"));

        var result = await _sut.Handle(
            new EstimateStoryPointsQuery("Fix bug", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Unavailable");
    }

    [Fact]
    public async Task Handle_WhenServiceMisconfigured_ReturnsNotConfiguredError()
    {
        _ai.EstimateStoryPointsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new InvalidOperationException("Anthropic API key not configured."));

        var result = await _sut.Handle(
            new EstimateStoryPointsQuery("Fix bug", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.NotConfigured");
    }
}
