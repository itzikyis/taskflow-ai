using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskFlow.Application.DevelopmentLinks.Commands.IngestGitHubEvent;
using TaskFlow.Application.DevelopmentLinks.GitHub;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.DevelopmentLinks;

/// <summary>Tests for <see cref="IngestGitHubEventCommandHandler"/>.</summary>
public sealed class IngestGitHubEventCommandHandlerTests
{
    private readonly IGitHubWebhookParser _parser = Substitute.For<IGitHubWebhookParser>();
    private readonly IDevelopmentLinkRepository _repo = Substitute.For<IDevelopmentLinkRepository>();
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly IngestGitHubEventCommandHandler _sut;

    public IngestGitHubEventCommandHandlerTests() =>
        _sut = new IngestGitHubEventCommandHandler(
            _parser, _repo, _tasks, NullLogger<IngestGitHubEventCommandHandler>.Instance);

    private ParsedDevelopmentRef RefFor(Guid taskId, string externalId = "7") => new(
        Repository: "owner/repo",
        RefType: DevelopmentRefType.PullRequest,
        Title: "Add login",
        Url: "https://github.com/owner/repo/pull/7",
        Status: DevelopmentLinkStatus.Open,
        ExternalId: externalId,
        TextToScan: $"feature/{taskId}-login");

    [Fact]
    public async Task Handle_ReferencedExistingTask_CreatesLink()
    {
        var task = TaskItem.Create("Task", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        _parser.Parse("pull_request", Arg.Any<string>()).Returns(new[] { RefFor(task.Id) });
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _repo.FindByExternalRefAsync(task.Id, "owner/repo", "7", Arg.Any<CancellationToken>())
             .Returns((TaskDevelopmentLink?)null);

        var result = await _sut.Handle(new IngestGitHubEventCommand("pull_request", "{}"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        await _repo.Received(1).AddAsync(Arg.Any<TaskDevelopmentLink>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReferencedUnknownTask_CreatesNothing()
    {
        var unknownId = Guid.NewGuid();
        _parser.Parse("pull_request", Arg.Any<string>()).Returns(new[] { RefFor(unknownId) });
        _tasks.GetByIdAsync(unknownId, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var result = await _sut.Handle(new IngestGitHubEventCommand("pull_request", "{}"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await _repo.DidNotReceive().AddAsync(Arg.Any<TaskDevelopmentLink>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingExternalRef_UpdatesInsteadOfDuplicating()
    {
        var task = TaskItem.Create("Task", null, TaskPriority.Medium, Guid.NewGuid()).Value!;
        var existing = TaskDevelopmentLink.Create(
            task.Id, "owner/repo", DevelopmentRefType.PullRequest,
            "Add login", "https://github.com/owner/repo/pull/7",
            DevelopmentLinkStatus.Open, "7").Value!;

        // Parser now reports the PR as merged.
        var mergedRef = RefFor(task.Id) with { Status = DevelopmentLinkStatus.Merged };
        _parser.Parse("pull_request", Arg.Any<string>()).Returns(new[] { mergedRef });
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _repo.FindByExternalRefAsync(task.Id, "owner/repo", "7", Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _sut.Handle(new IngestGitHubEventCommand("pull_request", "{}"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        existing.Status.Should().Be(DevelopmentLinkStatus.Merged);
        await _repo.DidNotReceive().AddAsync(Arg.Any<TaskDevelopmentLink>(), Arg.Any<CancellationToken>());
    }
}
