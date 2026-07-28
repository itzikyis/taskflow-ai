namespace TaskFlow.Application.TaskTemplates.Dtos;

/// <summary>Read model returned by task-template queries.</summary>
/// <param name="Id">Unique identifier of the template.</param>
/// <param name="ProjectId">Project the template belongs to.</param>
/// <param name="Name">Display name of the template.</param>
/// <param name="DefaultTitle">Default title applied to tasks created from this template.</param>
/// <param name="DefaultDescription">Optional default description.</param>
/// <param name="DefaultPriority">Optional default priority string.</param>
/// <param name="DefaultEstimatedHours">Optional default hour estimate.</param>
public sealed record TaskTemplateDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string DefaultTitle,
    string? DefaultDescription,
    string? DefaultPriority,
    int? DefaultEstimatedHours);
