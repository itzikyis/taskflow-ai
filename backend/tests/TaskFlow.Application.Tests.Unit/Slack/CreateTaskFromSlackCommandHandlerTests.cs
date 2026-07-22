using FluentAssertions;
using MediatR;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Slack;
using TaskFlow.Application.Slack.Commands.CreateTaskFromSlack;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Slack;

/// <summary>Unit tests for <see cref="CreateTaskFromSlackCommandHandler"/>.</summary>
public sealed class CreateTaskFromSlackCommandHandlerTests
{
    private static readonly Guid SystemUserId = Guid.NewGuid();

    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly ISlackOptions _slackOptions = Substitute.For<ISlackOptions>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CreateTaskFromSlackCommandHandler _sut;

    public CreateTaskFromSlackCommandHandlerTests()
    {
        _slackOptions.SystemUserId.Returns(SystemUserId);
        _slackOptions.SigningSecret.Returns((string?)null);
        _taskRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _sut = new CreateTaskFromSlackCommandHandler(_taskRepository, _slackOptions, _mediator);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithTaskId()
    {
        // Arrange
        var command = new CreateTaskFromSlackCommand(
            Title: "Fix the login bug",
            SlackUserId: "U12345",
            SlackUserName: "alice",
            ChannelId: "C99999");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAndSave()
    {
        // Arrange
        var command = new CreateTaskFromSlackCommand(
            Title: "Deploy hotfix",
            SlackUserId: "U00001",
            SlackUserName: "bob",
            ChannelId: "C11111");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _taskRepository.Received(1).AddAsync(
            Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
        await _taskRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SystemUserIdNotConfigured_ReturnsFailure()
    {
        // Arrange
        _slackOptions.SystemUserId.Returns(Guid.Empty);

        var command = new CreateTaskFromSlackCommand(
            Title: "Some task",
            SlackUserId: "U00002",
            SlackUserName: "carol",
            ChannelId: "C22222");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SlackErrors.SystemUserNotConfigured.Code);
    }

    [Fact]
    public async Task Handle_ValidCommand_DescriptionContainsSlackUserAndChannel()
    {
        // Arrange
        var capturedTask = (TaskItem?)null;
        await _taskRepository.AddAsync(
            Arg.Do<TaskItem>(t => capturedTask = t),
            Arg.Any<CancellationToken>());

        var command = new CreateTaskFromSlackCommand(
            Title: "Update docs",
            SlackUserId: "U99999",
            SlackUserName: "dave",
            ChannelId: "C55555");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedTask.Should().NotBeNull();
        capturedTask!.Description.Should().Contain("@dave");
        capturedTask.Description.Should().Contain("C55555");
    }

    [Fact]
    public async Task Handle_ActivityLogFailure_DoesNotPropagateException()
    {
        // Arrange
        _mediator
            .Send(Arg.Any<IRequest<Result<Guid>>>(), Arg.Any<CancellationToken>())
            .Returns<Result<Guid>>(_ => throw new InvalidOperationException("logging service down"));

        var command = new CreateTaskFromSlackCommand(
            Title: "Resilient task",
            SlackUserId: "U11111",
            SlackUserName: "eve",
            ChannelId: "C33333");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert — task still created even if logging fails
        result.IsSuccess.Should().BeTrue();
    }
}
