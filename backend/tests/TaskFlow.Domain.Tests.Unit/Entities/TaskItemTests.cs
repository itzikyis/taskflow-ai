using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Domain.Tests.Unit.Entities;

public sealed class TaskItemTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessResult()
    {
        var result = TaskItem.Create("Fix bug", "Details here", TaskPriority.High, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Fix bug");
        result.Value.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ReturnsFailure(string title)
    {
        var result = TaskItem.Create(title, null, TaskPriority.Low, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Task.TitleRequired");
    }

    [Fact]
    public void Create_WithTitleExceeding200Chars_ReturnsFailure()
    {
        var longTitle = new string('x', 201);

        var result = TaskItem.Create(longTitle, null, TaskPriority.Low, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Task.TitleTooLong");
    }

    [Fact]
    public void Create_RaisesDomainEvent()
    {
        var result = TaskItem.Create("My task", null, TaskPriority.Medium, Guid.NewGuid());

        result.Value!.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void TransitionTo_ValidTransition_Succeeds()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Low, Guid.NewGuid()).Value!;

        var result = task.TransitionTo(TaskItemStatus.InProgress);

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ReturnsFailure()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Low, Guid.NewGuid()).Value!;

        var result = task.TransitionTo(TaskItemStatus.Done); // cannot skip InProgress

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Task.InvalidTransition");
    }

    [Fact]
    public void SetDueDate_InPast_ReturnsFailure()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Low, Guid.NewGuid()).Value!;

        var result = task.SetDueDate(DateTime.UtcNow.AddDays(-1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Task.DueDateInPast");
    }
}
