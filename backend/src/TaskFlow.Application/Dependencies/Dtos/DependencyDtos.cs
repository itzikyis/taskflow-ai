namespace TaskFlow.Application.Dependencies.Dtos;

/// <summary>A raw dependency edge (used by the timeline view).</summary>
public sealed record DependencyEdgeDto(Guid Id, Guid TaskId, Guid BlockedByTaskId);

/// <summary>A related task shown in a dependency list.</summary>
public sealed record DependencyTaskDto(Guid DependencyId, Guid TaskId, string Title, string Status);

/// <summary>The blockers and blocked tasks for a given task.</summary>
public sealed record TaskDependenciesDto(
    Guid TaskId,
    IReadOnlyList<DependencyTaskDto> BlockedBy,
    IReadOnlyList<DependencyTaskDto> Blocks);
