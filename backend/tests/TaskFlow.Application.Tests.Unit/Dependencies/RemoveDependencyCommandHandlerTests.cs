using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Dependencies.Commands.RemoveDependency;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Dependencies;

public sealed class RemoveDependencyCommandHandlerTests
{
    private readonly ITaskDependencyRepository _repo = Substitute.For<ITaskDependencyRepository>();
    private readonly RemoveDependencyCommandHandler _sut;

    public RemoveDependencyCommandHandlerTests()
    {
        _sut = new RemoveDependencyCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ExistingDependency_RemovesAndSaves()
    {
        var dep = TaskDependency.Create(Guid.NewGuid(), Guid.NewGuid()).Value!;
        _repo.GetByIdAsync(dep.Id, Arg.Any<CancellationToken>()).Returns(dep);

        var result = await _sut.Handle(new RemoveDependencyCommand(dep.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Remove(dep);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskDependency?)null);

        var result = await _sut.Handle(new RemoveDependencyCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskDependency.NotFound");
    }
}
