using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TaskTemplates.Commands.CreateTaskTemplate;

/// <summary>Creates a new task template in the given project.</summary>
/// <param name="ProjectId">The project this template belongs to.</param>
/// <param name="Name">Display name for the template.</param>
/// <param name="DefaultTitle">Default title for tasks created from this template.</param>
/// <param name="DefaultDescription">Optional default description.</param>
/// <param name="DefaultPriority">Optional default priority ("Low", "Medium", "High", "Critical").</param>
/// <param name="DefaultEstimatedHours">Optional default hour estimate.</param>
public sealed record CreateTaskTemplateCommand(
    Guid ProjectId,
    string Name,
    string DefaultTitle,
    string? DefaultDescription,
    string? DefaultPriority,
    int? DefaultEstimatedHours)
    : IRequest<Result<Guid>>;
