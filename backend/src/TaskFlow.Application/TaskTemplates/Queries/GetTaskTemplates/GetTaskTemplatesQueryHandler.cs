using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.TaskTemplates.Dtos;

namespace TaskFlow.Application.TaskTemplates.Queries.GetTaskTemplates;

/// <summary>Handles <see cref="GetTaskTemplatesQuery"/>.</summary>
public sealed class GetTaskTemplatesQueryHandler(ITaskTemplateRepository repository)
    : IRequestHandler<GetTaskTemplatesQuery, IReadOnlyList<TaskTemplateDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskTemplateDto>> Handle(
        GetTaskTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await repository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        return templates
            .Select(t => new TaskTemplateDto(
                t.Id,
                t.ProjectId,
                t.Name,
                t.DefaultTitle,
                t.DefaultDescription,
                t.DefaultPriority,
                t.DefaultEstimatedHours))
            .ToList()
            .AsReadOnly();
    }
}
