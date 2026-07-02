using MediatR;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.MoveTaskToColumn;

/// <summary>Handles <see cref="MoveTaskToColumnCommand"/>.</summary>
internal sealed class MoveTaskToColumnCommandHandler(ITaskRepository taskRepository, IMediator mediator)
    : IRequestHandler<MoveTaskToColumnCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(MoveTaskToColumnCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        task.MoveToColumn(request.ColumnId);
        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await mediator.Send(new LogActivityCommand(
                request.ActorId,
                ActivityAction.MovedToColumn,
                "Task",
                request.TaskId,
                task.Title),
                cancellationToken);
        }
        catch
        {
            // Logging failure must never break the main operation.
        }

        return Result.Ok;
    }
}
