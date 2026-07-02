using MediatR;
using TaskFlow.Application.ActivityLogs.Commands.LogActivity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.UpdateTask;

/// <summary>Handles <see cref="UpdateTaskCommand"/>.</summary>
internal sealed class UpdateTaskCommandHandler(ITaskRepository taskRepository, IMediator mediator)
    : IRequestHandler<UpdateTaskCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var updateResult = task.UpdateDetails(request.Title, request.Description);
        if (updateResult.IsFailure)
            return updateResult;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await mediator.Send(new LogActivityCommand(
                request.ActorId,
                ActivityAction.Updated,
                "Task",
                request.TaskId,
                request.Title),
                cancellationToken);
        }
        catch
        {
            // Logging failure must never break the main operation.
        }

        return Result.Ok;
    }
}
