using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TaskTemplates.Commands.CreateTaskFromTemplate;

/// <summary>Creates a new task pre-populated from the given template.</summary>
/// <param name="TemplateId">The template to instantiate.</param>
/// <param name="CreatedByUserId">The user creating the task.</param>
public sealed record CreateTaskFromTemplateCommand(Guid TemplateId, Guid CreatedByUserId)
    : IRequest<Result<Guid>>;
