namespace TaskFlow.Application.DevelopmentLinks.Dtos;

/// <summary>DTO representing a task-to-code development link.</summary>
public sealed record DevelopmentLinkDto(
    Guid Id,
    Guid TaskId,
    string Repository,
    string RefType,
    string Title,
    string Url,
    string Status,
    string? ExternalId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
