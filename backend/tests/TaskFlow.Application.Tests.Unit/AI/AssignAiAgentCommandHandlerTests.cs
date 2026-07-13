using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskFlow.Application.AI;
using TaskFlow.Application.AI.Commands.AssignAiAgent;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.AI;

public sealed class AssignAiAgentCommandHandlerTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly ICommentRepository _comments = Substitute.For<ICommentRepository>();
    private readonly IAiAssistantService _ai = Substitute.For<IAiAssistantService>();
    private readonly AssignAiAgentCommandHandler _sut;

    public AssignAiAgentCommandHandlerTests() =>
        _sut = new AssignAiAgentCommandHandler(_tasks, _comments, _ai, NullLogger<AssignAiAgentCommandHandler>.Instance);

    [Fact]
    public async Task Handle_AssignsAgentAndPostsDraftComment()
    {
        var task = TaskItem.Create("Fix login", "auth bug", TaskPriority.High, Guid.NewGuid()).Value!;
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _ai.DraftTaskApproachAsync(task.Title, task.Description, Arg.Any<CancellationToken>())
           .Returns("- Reproduce the bug\n- Add a failing test\n- Fix and verify");

        var result = await _sut.Handle(new AssignAiAgentCommand(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.AssignedToUserId.Should().Be(AiAgent.Id);
        await _comments.Received(1).AddAsync(
            Arg.Is<Comment>(c => c.AuthorId == AiAgent.Id && c.TaskId == task.Id),
            Arg.Any<CancellationToken>());
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAiFails_StillAssignsAndPostsFallbackComment()
    {
        var task = TaskItem.Create("Fix login", null, TaskPriority.High, Guid.NewGuid()).Value!;
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _ai.DraftTaskApproachAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new HttpRequestException("no key"));

        var result = await _sut.Handle(new AssignAiAgentCommand(task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.AssignedToUserId.Should().Be(AiAgent.Id);
        await _comments.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingTask_ReturnsNotFound()
    {
        _tasks.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        var result = await _sut.Handle(new AssignAiAgentCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TaskErrors.NotFound.Code);
    }
}
