using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Queries.GenerateReleaseNotes;
using TaskFlow.Application.Interfaces;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class GenerateReleaseNotesQueryHandlerTests
{
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly GenerateReleaseNotesQueryHandler _sut;

    public GenerateReleaseNotesQueryHandlerTests() => _sut = new GenerateReleaseNotesQueryHandler(_ai);

    [Fact]
    public async Task Handle_WhenAiReturnsNotes_ReturnsSuccessWithMarkdownContent()
    {
        // Arrange
        var notes = new ReleaseNotes(
            "1.0.0",
            "This release ships the core task management features.",
            ["Task creation", "Kanban board"],
            ["Fixed login redirect"],
            ["Improved drag-and-drop"],
            "# Release Notes v1.0.0\n\n## Summary\nThis release ships the core task management features.");

        var tasks = new List<CompletedTaskSummary>
        {
            new("Add task creation", "Allow users to create tasks", "High"),
            new("Fix login redirect", null, "Medium")
        };

        _ai.GenerateReleaseNotesAsync(
                "1.0.0",
                Arg.Any<IEnumerable<(string Title, string? Description, string Priority)>>(),
                Arg.Any<CancellationToken>())
           .Returns(notes);

        // Act
        var result = await _sut.Handle(
            new GenerateReleaseNotesQuery("1.0.0", tasks),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be("1.0.0");
        result.Value.MarkdownContent.Should().Contain("# Release Notes v1.0.0");
        result.Value.Features.Should().HaveCount(2);
        result.Value.BugFixes.Should().ContainSingle();
        await _ai.Received(1).GenerateReleaseNotesAsync(
            "1.0.0",
            Arg.Any<IEnumerable<(string Title, string? Description, string Priority)>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAiServiceThrows_ReturnsFailureResult()
    {
        // Arrange
        _ai.GenerateReleaseNotesAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<(string Title, string? Description, string Priority)>>(),
                Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("AI service unreachable"));

        var tasks = new List<CompletedTaskSummary> { new("Some task", null, "Low") };

        // Act
        var result = await _sut.Handle(
            new GenerateReleaseNotesQuery("2.0.0", tasks),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Unavailable");
    }
}
