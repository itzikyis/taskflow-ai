using FluentAssertions;
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
    private readonly LogActivityCommandHandler _sut;

    public LogActivityCommandHandlerTests() =>
        _sut = new LogActivityCommandHandler(_repository);

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
}
