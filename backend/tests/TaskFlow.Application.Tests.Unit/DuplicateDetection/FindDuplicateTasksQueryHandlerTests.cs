using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.DuplicateDetection;
using TaskFlow.Application.DuplicateDetection.Queries.FindDuplicateTasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.DuplicateDetection;

public sealed class FindDuplicateTasksQueryHandlerTests
{
    private readonly ITaskRepository _repo = Substitute.For<ITaskRepository>();
    private readonly IDuplicateTaskDetectionService _detector = Substitute.For<IDuplicateTaskDetectionService>();
    private readonly FindDuplicateTasksQueryHandler _sut;

    public FindDuplicateTasksQueryHandlerTests() => _sut = new FindDuplicateTasksQueryHandler(_repo, _detector);

    [Fact]
    public async Task Handle_EmptyTitle_ReturnsEmptyWithoutQuerying()
    {
        var result = await _sut.Handle(new FindDuplicateTasksQuery("  ", null), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.DidNotReceive().GetAllAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExcludesDoneTasksAndSelf_ThenMapsMatches()
    {
        var self = TaskItem.Create("Candidate", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        var openTask = TaskItem.Create("Open similar", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        var doneTask = TaskItem.Create("Done similar", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        doneTask.TransitionTo(TaskItemStatus.InProgress);
        doneTask.TransitionTo(TaskItemStatus.Done);

        _repo.GetAllAsync(null, Arg.Any<CancellationToken>()).Returns(new List<TaskItem> { self, openTask, doneTask });

        _detector
            .FindDuplicates(
                "Candidate", null,
                Arg.Any<IEnumerable<(Guid, string, string?)>>(),
                Arg.Any<double>())
            .Returns(new List<DuplicateMatch> { new(openTask.Id, "Open similar", 0.8) });

        var result = await _sut.Handle(
            new FindDuplicateTasksQuery("Candidate", null, self.Id), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].TaskId.Should().Be(openTask.Id);
        result[0].Score.Should().Be(0.8);

        // Detector must have been given exactly the open, non-self task.
        _detector.Received(1).FindDuplicates(
            "Candidate", null,
            Arg.Is<IEnumerable<(Guid Id, string Title, string? Description)>>(
                c => c.Count() == 1 && c.First().Id == openTask.Id),
            Arg.Any<double>());
    }
}
