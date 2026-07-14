namespace TaskFlow.Application.ProjectDocs.Dtos;

/// <summary>Read model for a project document.</summary>
public sealed record ProjectDocumentDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Body,
    Guid AuthorId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
