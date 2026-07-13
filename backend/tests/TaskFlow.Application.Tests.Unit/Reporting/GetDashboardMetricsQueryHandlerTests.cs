using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Reporting.Queries.GetDashboardMetrics;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Reporting;

public sealed class GetDashboardMetricsQueryHandlerTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly GetDashboardMetricsQueryHandler _sut;

    public GetDashboardMetricsQueryHandlerTests() => _sut = new GetDashboardMetricsQueryHandler(_tasks);

    private static TaskItem Make(TaskItemStatus status, TaskPriority priority, Guid? assignee = null)
    {
        var t = TaskItem.Create("T", null, priority, Guid.NewGuid()).Value!;
        if (assignee is { } a) t.AssignTo(a);
        if (status == TaskItemStatus.InProgress) t.TransitionTo(TaskItemStatus.InProgress);
        if (status == TaskItemStatus.InReview) { t.TransitionTo(TaskItemStatus.InProgress); t.TransitionTo(TaskItemStatus.InReview); }
        if (status == TaskItemStatus.Done) { t.TransitionTo(TaskItemStatus.InProgress); t.TransitionTo(TaskItemStatus.Done); }
        return t;
    }

    [Fact]
    public async Task Handle_AggregatesStatusPriorityAndWorkload()
    {
        var alice = Guid.NewGuid();
        var list = new List<TaskItem>
        {
            Make(TaskItemStatus.Todo, TaskPriority.High, alice),
            Make(TaskItemStatus.InProgress, TaskPriority.High, alice),
            Make(TaskItemStatus.Done, TaskPriority.Low, alice),
            Make(TaskItemStatus.Todo, TaskPriority.Critical),
            Make(TaskItemStatus.InReview, TaskPriority.Medium, alice),
        };
        _tasks.GetAllAsync(null, Arg.Any<CancellationToken>()).Returns(list);

        var result = await _sut.Handle(new GetDashboardMetricsQuery(), CancellationToken.None);

        result.Total.Should().Be(5);
        result.Todo.Should().Be(2);
        result.InProgress.Should().Be(1);
        result.InReview.Should().Be(1);
        result.Done.Should().Be(1);
        result.High.Should().Be(2);
        result.Critical.Should().Be(1);

        // Workload counts only open tasks: Alice has 3 open (Todo, InProgress, InReview),
        // one unassigned open.
        result.Workload.Should().ContainSingle(w => w.UserId == alice && w.OpenTasks == 3);
        result.Workload.Should().ContainSingle(w => w.UserId == null && w.OpenTasks == 1);

        // One Done task -> one completion-trend point.
        result.CompletionTrend.Should().ContainSingle().Which.Completed.Should().Be(1);
    }
}
