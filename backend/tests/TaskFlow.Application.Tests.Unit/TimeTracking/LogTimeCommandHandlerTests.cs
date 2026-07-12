using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.TimeTracking.Commands.LogTime;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.TimeTracking;

public sealed class LogTimeCommandHandlerTests
{
    private readonly ITimeEntryRepository _repo = Substitute.For<ITimeEntryRepository>();
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly LogTimeCommandHandler _sut;

    public LogTimeCommandHandlerTests() => _sut = new LogTimeCommandHandler(_repo, _tasks);

    [Fact]
    public async Task Handle_ValidEntry_PersistsAndReturnsId()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var result = await _sut.Handle(
            new LogTimeCommand(task.Id, Guid.NewGuid(), 90, "pairing"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingTask_ReturnsNotFound()
    {
        _tasks.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var result = await _sut.Handle(
            new LogTimeCommand(Guid.NewGuid(), Guid.NewGuid(), 30, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TaskErrors.NotFound.Code);
        await _repo.DidNotReceive().AddAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonPositiveMinutes_ReturnsValidationError()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var result = await _sut.Handle(
            new LogTimeCommand(task.Id, Guid.NewGuid(), 0, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TimeEntry.MinutesMustBePositive");
    }
}
