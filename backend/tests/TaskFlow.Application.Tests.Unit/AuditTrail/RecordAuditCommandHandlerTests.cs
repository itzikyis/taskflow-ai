using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.AuditTrail.Commands.RecordAudit;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AuditTrail;

/// <summary>Unit tests for <see cref="RecordAuditCommandHandler"/>.</summary>
public sealed class RecordAuditCommandHandlerTests
{
    private readonly IAuditRepository _repository = Substitute.For<IAuditRepository>();
    private readonly RecordAuditCommandHandler _sut;

    public RecordAuditCommandHandlerTests() =>
        _sut = new RecordAuditCommandHandler(_repository);

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistEntryAndReturnId()
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new RecordAuditCommand(
            Guid.NewGuid(), "Task", Guid.NewGuid(), "Created");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithChanges_ShouldIncludeChangesJson()
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        const string changes = """{"title":{"from":"Old","to":"New"}}""";

        var command = new RecordAuditCommand(
            Guid.NewGuid(), "Task", Guid.NewGuid(), "Updated", changes);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddAsync(
            Arg.Is<AuditEntry>(e => e.Changes == changes),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Created")]
    [InlineData("Updated")]
    [InlineData("Deleted")]
    public async Task Handle_AnyValidAction_ShouldSucceed(string action)
    {
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new RecordAuditCommand(Guid.NewGuid(), "Task", Guid.NewGuid(), action);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
