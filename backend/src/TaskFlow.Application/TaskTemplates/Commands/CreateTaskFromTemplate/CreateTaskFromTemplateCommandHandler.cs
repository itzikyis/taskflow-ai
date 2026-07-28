using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.TaskTemplates.Commands.CreateTaskFromTemplate;

/// <summary>Handles <see cref="CreateTaskFromTemplateCommand"/>.</summary>
public sealed class CreateTaskFromTemplateCommandHandler(
    ITaskTemplateRepository templateRepository,
    ITaskRepository taskRepository)
    : IRequestHandler<CreateTaskFromTemplateCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        CreateTaskFromTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
            return Result<Guid>.Failure(TaskTemplateErrors.NotFound);

        var priority = Enum.TryParse<TaskPriority>(template.DefaultPriority, out var p)
            ? p
            : TaskPriority.Medium;

        var taskResult = TaskItem.Create(
            template.DefaultTitle,
            template.DefaultDescription,
            priority,
            request.CreatedByUserId);

        if (taskResult.IsFailure)
            return Result<Guid>.Failure(taskResult.Error);

        taskResult.Value!.SetTemplateId(template.Id);

        await taskRepository.AddAsync(taskResult.Value, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(taskResult.Value.Id);
    }
}
