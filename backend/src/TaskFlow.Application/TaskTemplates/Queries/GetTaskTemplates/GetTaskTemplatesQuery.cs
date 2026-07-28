using MediatR;
using TaskFlow.Application.TaskTemplates.Dtos;

namespace TaskFlow.Application.TaskTemplates.Queries.GetTaskTemplates;

/// <summary>Returns all task templates for the specified project.</summary>
/// <param name="ProjectId">The project whose templates should be returned.</param>
public sealed record GetTaskTemplatesQuery(Guid ProjectId)
    : IRequest<IReadOnlyList<TaskTemplateDto>>;
