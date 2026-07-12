using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Dependencies.Commands.AddDependency;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Dependencies;

public sealed class AddDependencyCommandHandlerTests
{
    private readonly ITaskDependencyRepository _repo = Substitute.For<ITaskDependencyRepository>();
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly AddDependencyCommandHandler _sut;

    public AddDependencyCommandHandlerTests()
    {
        _sut = new AddDependencyCommandHandler(_repo, _tasks);
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<TaskDependency>());
    }

    private TaskItem Task() => TaskItem.Create("T", null, TaskPriority.Medium, Guid.NewGuid()).Value!;

    private void BothExist(Guid a, Guid b)
    {
        _tasks.GetByIdAsync(a, Arg.Any<CancellationToken>()).Returns(TaskItem.Create("A", null, TaskPriority.Low, Guid.NewGuid()).Value);
        _tasks.GetByIdAsync(b, Arg.Any<CancellationToken>()).Returns(TaskItem.Create("B", null, TaskPriority.Low, Guid.NewGuid()).Value);
    }

    [Fact]
    public async Task Handle_ValidDependency_Persists()
    {
        var t = Guid.NewGuid(); var b = Guid.NewGuid();
        BothExist(t, b);

        var result = await _sut.Handle(new AddDependencyCommand(t, b), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<TaskDependency>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SelfDependency_Fails()
    {
        var t = Guid.NewGuid();
        var result = await _sut.Handle(new AddDependencyCommand(t, t), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskDependency.Self");
    }

    [Fact]
    public async Task Handle_Duplicate_Fails()
    {
        var t = Guid.NewGuid(); var b = Guid.NewGuid();
        BothExist(t, b);
        _repo.ExistsAsync(t, b, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.Handle(new AddDependencyCommand(t, b), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskDependency.Duplicate");
    }

    [Fact]
    public async Task Handle_Cycle_Fails()
    {
        // Existing: A is blocked by B  (edge B -> A). Adding: B blocked by A (edge A -> B) -> cycle.
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        BothExist(b, a);
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(new List<TaskDependency> { TaskDependency.Create(a, b).Value! });

        var result = await _sut.Handle(new AddDependencyCommand(b, a), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskDependency.Cycle");
    }
}
