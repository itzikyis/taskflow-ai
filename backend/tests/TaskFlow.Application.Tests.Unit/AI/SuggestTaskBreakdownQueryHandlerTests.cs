using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.SuggestTaskBreakdown;
using TaskFlow.Application.Interfaces;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class SuggestTaskBreakdownQueryHandlerTests
{
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly SuggestTaskBreakdownQueryHandler _sut;

    public SuggestTaskBreakdownQueryHandlerTests() =>
        _sut = new SuggestTaskBreakdownQueryHandler(_ai, NullLogger<SuggestTaskBreakdownQueryHandler>.Instance);

    [Fact]
    public async Task Handle_ReturnsSuggestionsFromAi()
    {
        IReadOnlyList<SubtaskSuggestion> suggestions =
        [
            new("Design the schema", "Model the onboarding tables"),
            new("Build the API", null),
        ];
        _ai.GenerateSubtasksAsync("Redesign onboarding", Arg.Any<string?>(), Arg.Any<CancellationToken>())
           .Returns(suggestions);

        var result = await _sut.Handle(new SuggestTaskBreakdownQuery("Redesign onboarding", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Title.Should().Be("Design the schema");
    }

    [Fact]
    public async Task Handle_WhenAiThrows_ReturnsUnavailable()
    {
        _ai.GenerateSubtasksAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("boom"));

        var result = await _sut.Handle(new SuggestTaskBreakdownQuery("X", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Unavailable");
    }
}
