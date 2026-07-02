using System.Text.Json;
using MediatR;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.AuditTrail.Commands.RecordAudit;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.UpdateTaskStatus;

/// <summary>Handles <see cref="UpdateTaskStatusCommand"/>.</summary>
internal sealed class UpdateTaskStatusCommandHandler(ITaskRepository taskRepository, IMediator mediator)
    : IRequestHandler<UpdateTaskStatusCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var oldStatus = task.Status;

        var result = task.TransitionTo(request.NewStatus);
        if (result.IsFailure)
            return result;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await mediator.Send(new LogActivityCommand(
                request.ActorId,
                ActivityAction.StatusChanged,
                "Task",
                request.TaskId,
                task.Title),
                cancellationToken);
        }
        catch
        {
            // Logging failure must never break the main operation.
        }

        try
        {
            var changes = JsonSerializer.Serialize(new
            {
                status = new { from = oldStatus.ToString(), to = request.NewStatus.ToString() }
            });
            await mediator.Send(new RecordAuditCommand(
                request.ActorId,
                "Task",
                request.TaskId,
                "Updated",
                changes),
                cancellationToken);
        }
        catch
        {
            // Audit failure must never break the main operation.
        }

        return Result.Ok;
    }
}
