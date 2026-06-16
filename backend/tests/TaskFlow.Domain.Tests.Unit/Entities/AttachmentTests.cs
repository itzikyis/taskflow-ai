using FluentAssertions;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Unit.Entities;

public sealed class AttachmentTests
{
    private static readonly Guid TaskId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Attachment.Create(TaskId, UserId, "report.pdf", "application/pdf", 1024, "https://storage/report.pdf");
        result.IsSuccess.Should().BeTrue();
        result.Value!.FileName.Should().Be("report.pdf");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyFileName_ReturnsFileNameRequired(string name)
    {
        var result = Attachment.Create(TaskId, UserId, name, "application/pdf", 1024, "https://storage/f");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.FileNameRequired");
    }

    [Fact]
    public void Create_WithFileNameOver255Chars_ReturnsFileNameTooLong()
    {
        var result = Attachment.Create(TaskId, UserId, new string('x', 256), "application/pdf", 1024, "https://storage/f");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.FileNameTooLong");
    }

    [Fact]
    public void Create_WithFileSizeOver100MB_ReturnsFileTooLarge()
    {
        var over100MB = 101L * 1024 * 1024;
        var result = Attachment.Create(TaskId, UserId, "big.zip", "application/zip", over100MB, "https://storage/big.zip");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.FileTooLarge");
    }

    [Fact]
    public void Create_RaisesAttachmentUploadedEvent()
    {
        var result = Attachment.Create(TaskId, UserId, "file.txt", "text/plain", 100, "https://storage/file.txt");
        result.Value!.DomainEvents.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyContentType_ReturnsContentTypeRequired(string ct)
    {
        var result = Attachment.Create(TaskId, UserId, "file.txt", ct, 100, "https://storage/file.txt");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.ContentTypeRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyStorageUrl_ReturnsStorageUrlRequired(string url)
    {
        var result = Attachment.Create(TaskId, UserId, "file.txt", "text/plain", 100, url);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.StorageUrlRequired");
    }
}
