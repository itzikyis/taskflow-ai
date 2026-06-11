namespace TaskFlow.Application.Projects.Queries.GetProjectById;

/// <summary>Read-model DTO returned by project queries.</summary>
public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
