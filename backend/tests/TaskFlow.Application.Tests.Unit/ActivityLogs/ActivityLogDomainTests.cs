using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Xunit;

namespace TaskFlow.Application.Tests.Unit.ActivityLogs;

/// <summary>Unit tests for the <see cref="ActivityLog"/> domain entity.</summary>
public sealed class ActivityLogDomainTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnLog()
    {
        var actorId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var log = ActivityLog.Create(
            actorId,
            ActivityAction.Created,
            "Task",
            entityId,
            "My Task",
            projectId,
            "{\"key\":\"value\"}");

        log.Id.Should().NotBeEmpty();
        log.ActorId.Should().Be(actorId);
        log.Action.Should().Be(ActivityAction.Created);
        log.EntityType.Should().Be("Task");
        log.EntityId.Should().Be(entityId);
        log.EntityName.Should().Be("My Task");
        log.ProjectId.Should().Be(projectId);
        log.Metadata.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public void Create_WithEmptyEntityType_ShouldThrow()
    {
        var act = () => ActivityLog.Create(
            Guid.NewGuid(),
            ActivityAction.Created,
            "  ",
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldSetOccurredAtToNow()
    {
        var before = DateTime.UtcNow;

        var log = ActivityLog.Create(
            Guid.NewGuid(),
            ActivityAction.Updated,
            "Task",
            Guid.NewGuid());

        log.OccurredAt.Should().BeOnOrAfter(before);
        log.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
