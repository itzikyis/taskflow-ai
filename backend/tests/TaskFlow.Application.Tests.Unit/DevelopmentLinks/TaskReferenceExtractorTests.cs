using FluentAssertions;
using TaskFlow.Application.DevelopmentLinks;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.DevelopmentLinks;

/// <summary>Tests for <see cref="TaskReferenceExtractor"/>.</summary>
public sealed class TaskReferenceExtractorTests
{
    [Fact]
    public void Extract_FindsGuidInBranchName()
    {
        var id = Guid.NewGuid();
        var result = TaskReferenceExtractor.Extract($"feature/{id}-fix-login");

        result.Should().ContainSingle().Which.Should().Be(id);
    }

    [Fact]
    public void Extract_FindsGuidInCommitMessage()
    {
        var id = Guid.NewGuid();
        var result = TaskReferenceExtractor.Extract($"Fixes {id} by adding null check");

        result.Should().ContainSingle().Which.Should().Be(id);
    }

    [Fact]
    public void Extract_DeduplicatesRepeatedReferences()
    {
        var id = Guid.NewGuid();
        var result = TaskReferenceExtractor.Extract($"{id} ... and again {id}");

        result.Should().ContainSingle().Which.Should().Be(id);
    }

    [Fact]
    public void Extract_FindsMultipleDistinctReferences()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var result = TaskReferenceExtractor.Extract($"Closes {a} and {b}");

        result.Should().BeEquivalentTo(new[] { a, b });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("just a normal commit with no task ref")]
    [InlineData("not-a-guid-1234")]
    public void Extract_ReturnsEmpty_WhenNoGuidPresent(string? text)
    {
        TaskReferenceExtractor.Extract(text).Should().BeEmpty();
    }
}
