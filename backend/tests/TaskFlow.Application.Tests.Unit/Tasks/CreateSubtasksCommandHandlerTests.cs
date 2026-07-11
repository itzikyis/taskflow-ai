using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Tasks.Commands.CreateSubtasks;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Tasks;

public sealed class CreateSubtasksCommandHandlerTests
{
    private readonly ITaskRepository _repo = Substitute.For<ITaskRepository>();
    private readonly CreateSubtasksCommandHandler _sut;

    public CreateSubtasksCommandHandlerTests() => _sut = new CreateSubtasksCommandHandler(_repo);

    [Fact]
    public async Task Handle_WhenParentExists_CreatesChildrenWithParentSet()
    {
        var parent = TaskItem.Create("Epic", null, TaskPriority.High, Guid.NewGuid()).Value!;
        _repo.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);

        var added = new List<TaskItem>();
        await _repo.AddAsync(Arg.Do<TaskItem>(t => added.Add(t)), Arg.Any<CancellationToken>());

        var command = new CreateSubtasksCommand(
            parent.Id,
            [new NewSubtaskInput("Sub A", "detail"), new NewSubtaskInput("Sub B", null)],
            Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        added.Should().HaveCount(2);
        added.Should().OnlyContain(t => t.ParentTaskId == parent.Id);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParentMissing_ReturnsNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var command = new CreateSubtasksCommand(
            Guid.NewGuid(), [new NewSubtaskInput("Sub", null)], Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TaskErrors.NotFound.Code);
        await _repo.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }
}
