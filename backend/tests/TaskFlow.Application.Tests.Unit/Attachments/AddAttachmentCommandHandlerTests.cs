using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.Attachments.Commands.AddAttachment;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.Attachments;

public sealed class AddAttachmentCommandHandlerTests
{
    private readonly IAttachmentRepository _repo = Substitute.For<IAttachmentRepository>();
    private readonly AddAttachmentCommandHandler _sut;

    public AddAttachmentCommandHandlerTests() => _sut = new AddAttachmentCommandHandler(_repo);

    [Fact]
    public async Task Handle_ValidCommand_AddsAttachmentAndReturnsId()
    {
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        var result = await _sut.Handle(
            new AddAttachmentCommand(Guid.NewGuid(), Guid.NewGuid(), "doc.pdf", "application/pdf", 2048, "https://storage/doc.pdf"),
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<Attachment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyFileName_ReturnsFailureWithoutPersisting()
    {
        var result = await _sut.Handle(
            new AddAttachmentCommand(Guid.NewGuid(), Guid.NewGuid(), "", "application/pdf", 2048, "https://storage/doc.pdf"),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.FileNameRequired");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Attachment>(), Arg.Any<CancellationToken>());
    }
}
