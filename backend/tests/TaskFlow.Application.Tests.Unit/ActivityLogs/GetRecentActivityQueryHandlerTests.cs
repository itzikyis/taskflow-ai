using FluentAssertions;
using NSubstitute;
using TaskFlow.Application.ActivityLogs.Queries.GetRecent;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.ActivityLogs;

/// <summary>Unit tests for <see cref="GetRecentActivityQueryHandler"/>.</summary>
public sealed class GetRecentActivityQueryHandlerTests
{
    private readonly IActivityLogRepository _repository = Substitute.For<IActivityLogRepository>();
    private readonly GetRecentActivityQueryHandler _sut;

    public GetRecentActivityQueryHandlerTests() =>
        _sut = new GetRecentActivityQueryHandler(_repository);

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos()
    {
        var actorId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var log = ActivityLog.Create(
            actorId,
            ActivityAction.Created,
            "Task",
            entityId,
            "Sample Task",
            projectId,
            null);

        _repository.GetRecentAsync(1, 50, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityLog> { log });

        var result = await _sut.Handle(new GetRecentActivityQuery(1, 50), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value![0];
        dto.ActorId.Should().Be(actorId);
        dto.EntityType.Should().Be("Task");
        dto.EntityId.Should().Be(entityId);
        dto.EntityName.Should().Be("Sample Task");
        dto.ProjectId.Should().Be(projectId);
        dto.Action.Should().Be(ActivityAction.Created.ToString());
    }

    [Fact]
    public async Task Handle_EmptyRepository_ShouldReturnEmptyList()
    {
        _repository.GetRecentAsync(1, 50, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityLog>());

        var result = await _sut.Handle(new GetRecentActivityQuery(1, 50), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
