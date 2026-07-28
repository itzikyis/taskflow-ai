using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TaskTemplates.Commands.DeleteTaskTemplate;

/// <summary>Permanently removes a task template by id.</summary>
/// <param name="TemplateId">The id of the template to delete.</param>
public sealed record DeleteTaskTemplateCommand(Guid TemplateId) : IRequest<Result>;
