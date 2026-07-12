using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Infrastructure.Tests.Integration;

/// <summary>Pure (no-DB) tests for <see cref="KeywordTaskSearchInterpreter"/>.</summary>
public sealed class KeywordTaskSearchInterpreterTests
{
    private readonly KeywordTaskSearchInterpreter _sut = new();

    [Fact]
    public void Interpret_OverdueHighPriorityAssignedToMe()
    {
        var f = _sut.Interpret("overdue high priority bugs assigned to me");

        f.Overdue.Should().BeTrue();
        f.Priority.Should().Be(TaskPriority.High);
        f.MineOnly.Should().BeTrue();
    }

    [Fact]
    public void Interpret_InProgressStatus()
    {
        _sut.Interpret("what is in progress").Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void Interpret_OpenOnly_WhenNoExplicitStatus()
    {
        var f = _sut.Interpret("open tasks");
        f.OpenOnly.Should().BeTrue();
        f.Status.Should().BeNull();
    }

    [Fact]
    public void Interpret_ExtractsKeywordsAfterAbout()
    {
        var f = _sut.Interpret("tasks about the login flow");
        f.Keywords.Should().Contain("login");
        f.Keywords.Should().NotContain("the");
        f.Keywords.Should().NotContain("flow"); // stop word
    }

    [Fact]
    public void Describe_ProducesReadableSummary()
    {
        var f = _sut.Interpret("overdue critical tasks assigned to me");
        var text = _sut.Describe(f);

        text.Should().Contain("Critical priority");
        text.Should().Contain("assigned to you");
        text.Should().Contain("overdue");
    }

    [Fact]
    public void Interpret_EmptyQuery_ReturnsEmptyFilter()
    {
        _sut.Interpret("   ").Should().Be(TaskSearchFilterEmptyMarker());
    }

    private static TaskFlow.Application.Search.TaskSearchFilter TaskSearchFilterEmptyMarker()
        => TaskFlow.Application.Search.TaskSearchFilter.Empty;
}
