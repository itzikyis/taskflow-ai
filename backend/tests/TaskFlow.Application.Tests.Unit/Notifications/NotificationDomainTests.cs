using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Notifications;

/// <summary>Unit tests for the <see cref="Notification"/> domain entity.</summary>
public sealed class NotificationDomainTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnNotification()
    {
        var userId = Guid.NewGuid();
        var relatedId = Guid.NewGuid();

        var notification = Notification.Create(
            userId, "Task assigned", "You have been assigned a task.",
            NotificationType.TaskAssigned, relatedId);

        notification.Id.Should().NotBeEmpty();
        notification.UserId.Should().Be(userId);
        notification.Title.Should().Be("Task assigned");
        notification.Message.Should().Be("You have been assigned a task.");
        notification.Type.Should().Be(NotificationType.TaskAssigned);
        notification.IsRead.Should().BeFalse();
        notification.RelatedEntityId.Should().Be(relatedId);
        notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrow()
    {
        var act = () => Notification.Create(
            Guid.NewGuid(), "  ", "Some message.", NotificationType.General);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAsRead_ShouldSetIsReadTrue()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), "Title", "Message.", NotificationType.General);

        notification.IsRead.Should().BeFalse();

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
    }
}
