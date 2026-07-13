using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.ActivityLogs;

/// <summary>Unit tests for <see cref="LogActivityCommandHandler"/>.</summary>
public sealed class LogActivityCommandHandlerTests
{
    private readonly IActivityLogRepository _repository = Substitute.For<IActivityLogRepository>();
    private readonly ISlackIntegrationRepository _slack = Substitute.For<ISlackIntegrationRepository>();
    private readonly IExternalNotificationService _notifier = Substitute.For<IExternalNotificationService>();
    private readonly LogActivityCommandHandler _sut;

    public LogActivityCommandHandlerTests() =>
        _sut = new LogActivityCommandHandler(
            _repository, _slack, _notifier, NullLogger<LogActivityCommandHandler>.Instance);

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateLogAndReturnId()
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new LogActivityCommand(
            Guid.NewGuid(),
            ActivityAction.Created,
            "Task",
            Guid.NewGuid(),
            "My Task");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallAddAndSave()
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new LogActivityCommand(
            Guid.NewGuid(),
            ActivityAction.Deleted,
            "Comment",
            Guid.NewGuid());

        await _sut.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Any<ActivityLog>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSlackEnabledForEvent_ForwardsMessage()
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var slack = SlackIntegration.Create("https://hooks.slack.com/services/x");
        _slack.GetAsync(Arg.Any<CancellationToken>()).Returns(slack);

        await _sut.Handle(
            new LogActivityCommand(Guid.NewGuid(), ActivityAction.Created, "Task", Guid.NewGuid(), "My Task"),
            CancellationToken.None);

        await _notifier.Received(1).SendAsync(
            "https://hooks.slack.com/services/x", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoSlackConfig_DoesNotForward()
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _slack.GetAsync(Arg.Any<CancellationToken>()).Returns((SlackIntegration?)null);

        await _sut.Handle(
            new LogActivityCommand(Guid.NewGuid(), ActivityAction.Created, "Task", Guid.NewGuid(), "My Task"),
            CancellationToken.None);

        await _notifier.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
