using FluentAssertions;
using Xunit;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Tasks.Commands.CreateTask;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tests.Unit.Tasks;

public sealed class CreateTaskCommandHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly CreateTaskCommandHandler _sut;

    public CreateTaskCommandHandlerTests()
    {
        _sut = new CreateTaskCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
    {
        var command = new CreateTaskCommand("Test task", null, TaskPriority.Medium, Guid.NewGuid());
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyTitle_ReturnsFailure()
    {
        var command = new CreateTaskCommand("", null, TaskPriority.Low, Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Task.TitleRequired");
        await _repository.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }
}
