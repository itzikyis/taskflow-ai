using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.DevelopmentLinks.Commands.LinkDevelopment;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.DevelopmentLinks;

/// <summary>Tests for <see cref="LinkDevelopmentCommandHandler"/>.</summary>
public sealed class LinkDevelopmentCommandHandlerTests
{
    private readonly IDevelopmentLinkRepository _repo = Substitute.For<IDevelopmentLinkRepository>();
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly LinkDevelopmentCommandHandler _sut;

    public LinkDevelopmentCommandHandlerTests() => _sut = new LinkDevelopmentCommandHandler(_repo, _tasks);

    [Fact]
    public async Task Handle_WhenTaskExists_CreatesLink()
    {
        var task = TaskItem.Create("Task", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var command = new LinkDevelopmentCommand(
            task.Id, "owner/repo", DevelopmentRefType.PullRequest,
            "Add login", "https://github.com/owner/repo/pull/7",
            DevelopmentLinkStatus.Open, "7");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<TaskDevelopmentLink>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        _tasks.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var command = new LinkDevelopmentCommand(
            Guid.NewGuid(), "owner/repo", DevelopmentRefType.Branch,
            "feature/x", "https://github.com/owner/repo/tree/feature/x");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TaskErrors.NotFound.Code);
        await _repo.DidNotReceive().AddAsync(Arg.Any<TaskDevelopmentLink>(), Arg.Any<CancellationToken>());
    }
}
