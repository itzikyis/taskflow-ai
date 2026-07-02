using MediatR;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.AuditTrail.Commands.RecordAudit;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.DeleteTask;

/// <summary>Handles <see cref="DeleteTaskCommand"/>.</summary>
internal sealed class DeleteTaskCommandHandler(ITaskRepository taskRepository, IMediator mediator)
    : IRequestHandler<DeleteTaskCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var taskTitle = task.Title;
        taskRepository.Remove(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await mediator.Send(new LogActivityCommand(
                request.ActorId,
                ActivityAction.Deleted,
                "Task",
                request.TaskId,
                taskTitle),
                cancellationToken);
        }
        catch
        {
            // Logging failure must never break the main operation.
        }

        try
        {
            await mediator.Send(new RecordAuditCommand(
                request.ActorId,
                "Task",
                request.TaskId,
                "Deleted"),
                cancellationToken);
        }
        catch
        {
            // Audit failure must never break the main operation.
        }

        return Result.Ok;
    }
}
