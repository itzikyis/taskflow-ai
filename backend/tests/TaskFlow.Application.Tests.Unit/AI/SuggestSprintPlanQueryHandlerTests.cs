using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.SuggestSprintPlan;
using TaskFlow.Application.Interfaces;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class SuggestSprintPlanQueryHandlerTests
{
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly SuggestSprintPlanQueryHandler _sut;

    public SuggestSprintPlanQueryHandlerTests() =>
        _sut = new SuggestSprintPlanQueryHandler(_ai, NullLogger<SuggestSprintPlanQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenAiReturnsSprintPlan_ReturnsSuccessResult()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedPlan = new SprintPlan(
            "Deliver user authentication",
            [new SprintTaskSuggestion(taskId, "Implement login", 5, "High priority security task")],
            "Selected high-priority auth tasks that fit within capacity.");

        _ai.SuggestSprintPlanAsync(Arg.Any<IEnumerable<(Guid, string, string?, string, string)>>(), 40, Arg.Any<CancellationToken>())
           .Returns(expectedPlan);

        var query = new SuggestSprintPlanQuery(
            [new TaskSummary(taskId, "Implement login", "Add OAuth2 login", "High", "Todo")],
            40);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.SprintGoal.Should().Be("Deliver user authentication");
        result.Value.SuggestedTasks.Should().HaveCount(1);
        result.Value.SuggestedTasks[0].TaskId.Should().Be(taskId);
        result.Value.SuggestedTasks[0].EstimatedPoints.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenAiServiceThrows_ReturnsFailureResult()
    {
        // Arrange
        _ai.SuggestSprintPlanAsync(
                Arg.Any<IEnumerable<(Guid, string, string?, string, string)>>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("AI service unreachable"));

        var query = new SuggestSprintPlanQuery(
            [new TaskSummary(Guid.NewGuid(), "Some task", null, "Medium", "Todo")],
            40);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Unavailable");
    }

    [Fact]
    public async Task Handle_WhenServiceMisconfigured_ReturnsNotConfiguredError()
    {
        _ai.SuggestSprintPlanAsync(
                Arg.Any<IEnumerable<(Guid, string, string?, string, string)>>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
           .ThrowsAsync(new InvalidOperationException("Anthropic API key not configured."));

        var query = new SuggestSprintPlanQuery(
            [new TaskSummary(Guid.NewGuid(), "Some task", null, "Medium", "Todo")],
            40);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.NotConfigured");
    }
}
