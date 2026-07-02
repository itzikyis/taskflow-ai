using MediatR;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.AuditTrail.Commands.RecordAudit;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.CreateTask;

/// <summary>Handles <see cref="CreateTaskCommand"/>.</summary>
public sealed class CreateTaskCommandHandler(ITaskRepository taskRepository, IMediator mediator)
    : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        var taskResult = TaskItem.Create(
            request.Title,
            request.Description,
            request.Priority,
            request.CreatedByUserId);

        if (taskResult.IsFailure)
            return Result<Guid>.Failure(taskResult.Error);

        await taskRepository.AddAsync(taskResult.Value!, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await mediator.Send(new LogActivityCommand(
                request.CreatedByUserId,
                ActivityAction.Created,
                "Task",
                taskResult.Value!.Id,
                taskResult.Value!.Title),
                cancellationToken);
        }
        catch
        {
            // Logging failure must never break the main operation.
        }

        try
        {
            await mediator.Send(new RecordAuditCommand(
                request.CreatedByUserId,
                "Task",
                taskResult.Value!.Id,
                "Created"),
                cancellationToken);
        }
        catch
        {
            // Audit failure must never break the main operation.
        }

        return Result<Guid>.Success(taskResult.Value!.Id);
    }
}
