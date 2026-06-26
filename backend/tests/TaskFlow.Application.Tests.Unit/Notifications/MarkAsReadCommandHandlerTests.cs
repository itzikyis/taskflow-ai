using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Notifications.Commands.MarkAsRead;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Notifications;

/// <summary>Unit tests for <see cref="MarkAsReadCommandHandler"/>.</summary>
public sealed class MarkAsReadCommandHandlerTests
{
    private readonly INotificationRepository _repo = Substitute.For<INotificationRepository>();
    private readonly MarkAsReadCommandHandler _sut;

    public MarkAsReadCommandHandlerTests() =>
        _sut = new MarkAsReadCommandHandler(_repo);

    [Fact]
    public async Task Handle_OwnNotification_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, "Title", "Msg", NotificationType.General);

        _repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new MarkAsReadCommand(notification.Id, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OtherUsersNotification_ShouldReturnNotOwner()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var notification = Notification.Create(ownerId, "Title", "Msg", NotificationType.General);

        _repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        var result = await _sut.Handle(
            new MarkAsReadCommand(notification.Id, requesterId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(NotificationErrors.NotOwner.Code);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var result = await _sut.Handle(
            new MarkAsReadCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(NotificationErrors.NotFound.Code);
    }
}
