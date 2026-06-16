using FluentAssertions;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Unit.Entities;

/// <summary>Unit tests for the <see cref="Comment"/> aggregate.</summary>
public sealed class CommentTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "Great work!");
        result.IsSuccess.Should().BeTrue();
        result.Value!.Content.Should().Be("Great work!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyContent_ReturnsFailure(string content)
    {
        var result = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), content);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ContentRequired");
    }

    [Fact]
    public void Create_WithContentOver5000Chars_ReturnsFailure()
    {
        var result = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), new string('x', 5001));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.ContentTooLong");
    }

    [Fact]
    public void Create_RaisesCommentAddedEvent()
    {
        var result = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");
        result.Value!.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Edit_ByAuthor_UpdatesContent()
    {
        var authorId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), authorId, "Original").Value!;
        var result = comment.Edit(authorId, "Edited");
        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be("Edited");
        comment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Edit_ByNonAuthor_ReturnsNotOwner()
    {
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "Original").Value!;
        var result = comment.Edit(Guid.NewGuid(), "Hacked");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotOwner");
    }
}
