using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Notifications.Commands.CreateNotification;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Notifications;

/// <summary>Unit tests for <see cref="CreateNotificationCommandHandler"/>.</summary>
public sealed class CreateNotificationCommandHandlerTests
{
    private readonly INotificationRepository _repo = Substitute.For<INotificationRepository>();
    private readonly CreateNotificationCommandHandler _sut;

    public CreateNotificationCommandHandlerTests() =>
        _sut = new CreateNotificationCommandHandler(_repo);

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnId()
    {
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new CreateNotificationCommand(
            Guid.NewGuid(), "Hello", "You have a new notification.", NotificationType.General);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallSaveChanges()
    {
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new CreateNotificationCommand(
            Guid.NewGuid(), "Hello", "You have a new notification.", NotificationType.General);

        await _sut.Handle(command, CancellationToken.None);

        await _repo.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
