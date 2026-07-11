using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Infrastructure.Tests.Integration;

/// <summary>Pure (no-DB) tests for <see cref="GitHubWebhookParser"/>.</summary>
public sealed class GitHubWebhookParserTests
{
    private readonly GitHubWebhookParser _sut = new();

    [Fact]
    public void Parse_PushEvent_ExtractsCommitReferences()
    {
        const string payload = """
        {
          "ref": "refs/heads/feature/login",
          "repository": { "full_name": "acme/taskflow" },
          "commits": [
            { "id": "abc123", "message": "Fix login bug", "url": "https://github.com/acme/taskflow/commit/abc123" }
          ]
        }
        """;

        var refs = _sut.Parse("push", payload);

        refs.Should().ContainSingle();
        var r = refs[0];
        r.Repository.Should().Be("acme/taskflow");
        r.RefType.Should().Be(DevelopmentRefType.Commit);
        r.Title.Should().Be("Fix login bug");
        r.ExternalId.Should().Be("abc123");
        r.Status.Should().Be(DevelopmentLinkStatus.None);
        r.TextToScan.Should().Contain("feature/login").And.Contain("Fix login bug");
    }

    [Theory]
    [InlineData("open", false, false, DevelopmentLinkStatus.Open)]
    [InlineData("open", false, true, DevelopmentLinkStatus.Draft)]
    [InlineData("closed", true, false, DevelopmentLinkStatus.Merged)]
    [InlineData("closed", false, false, DevelopmentLinkStatus.Closed)]
    public void Parse_PullRequest_MapsStatus(string state, bool merged, bool draft, DevelopmentLinkStatus expected)
    {
        var payload = $$"""
        {
          "action": "opened",
          "repository": { "full_name": "acme/taskflow" },
          "pull_request": {
            "number": 42,
            "title": "Add OAuth login",
            "body": "Implements the flow",
            "html_url": "https://github.com/acme/taskflow/pull/42",
            "state": "{{state}}",
            "merged": {{merged.ToString().ToLowerInvariant()}},
            "draft": {{draft.ToString().ToLowerInvariant()}},
            "head": { "ref": "feature/oauth" }
          }
        }
        """;

        var refs = _sut.Parse("pull_request", payload);

        refs.Should().ContainSingle();
        var r = refs[0];
        r.RefType.Should().Be(DevelopmentRefType.PullRequest);
        r.ExternalId.Should().Be("42");
        r.Status.Should().Be(expected);
        r.TextToScan.Should().Contain("feature/oauth").And.Contain("Add OAuth login");
    }

    [Fact]
    public void Parse_UnsupportedEvent_ReturnsEmpty()
    {
        _sut.Parse("issues", "{}").Should().BeEmpty();
    }
}
