using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.GenerateRetrospective;
using TaskFlow.Application.Interfaces;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class GenerateSprintRetrospectiveQueryHandlerTests
{
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly GenerateSprintRetrospectiveQueryHandler _sut;

    public GenerateSprintRetrospectiveQueryHandlerTests() =>
        _sut = new GenerateSprintRetrospectiveQueryHandler(
            _ai, NullLogger<GenerateSprintRetrospectiveQueryHandler>.Instance);

    private static GenerateSprintRetrospectiveQuery Query() => new(
        [new RetroTaskSummary("Ship login", "done", "High")],
        [new RetroTaskSummary("Refactor", null, "Low")]);

    [Fact]
    public async Task Handle_ReturnsRetrospectiveFromAi()
    {
        var retro = new SprintRetrospective(
            "Solid sprint.", ["Shipped login"], ["Refactor slipped"], ["Estimates close"], ["Timebox refactors"]);
        _ai.GenerateRetrospectiveAsync(
                Arg.Any<IEnumerable<(string, string?, string)>>(),
                Arg.Any<IEnumerable<(string, string?, string)>>(),
                Arg.Any<CancellationToken>())
           .Returns(retro);

        var result = await _sut.Handle(Query(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Summary.Should().Be("Solid sprint.");
        result.Value.WentWell.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenAiThrows_ReturnsUnavailable()
    {
        _ai.GenerateRetrospectiveAsync(
                Arg.Any<IEnumerable<(string, string?, string)>>(),
                Arg.Any<IEnumerable<(string, string?, string)>>(),
                Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("boom"));

        var result = await _sut.Handle(Query(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Unavailable");
    }
}
